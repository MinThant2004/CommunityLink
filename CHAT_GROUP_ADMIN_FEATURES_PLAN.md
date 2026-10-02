# Chat Group Creator/Admin Features — Implementation Plan

**Status:** Planning complete, awaiting execution
**Scope:** Creator/Admin group administration for Chat Groups, modeled on Telegram.

---

## 1. Summary

Eight Creator/Admin capabilities were requested. **Two already exist** and need hardening rather
than construction. The remaining six are new work spanning the database, domain services, REST
endpoints, the Blazor UI, and the test suite.

| # | Feature | Current state | Work |
|---|---------|---------------|------|
| 1 | View group members | Exists, **unauthorized** | Gate + tests |
| 2 | Remove inappropriate messages | Exists, correct | None |
| 3 | Remove member | Exists, **privilege gap** | Harden + tests |
| 4 | Edit chat-group information | Missing | New |
| 5 | Change chat-group image | Missing | New (storage + endpoint + UI) |
| 6 | Add members | Missing | New |
| 7 | Ban / remove member | Partial (remove only) | New (ban) + harden |
| 8 | Change chat-group fee | Missing | New |
| 9 | Delete chat-group | Missing | New |

### Decisions locked in

- **Role matrix:** OWNER = edit info, change image, add members, change fee, delete group, remove,
  ban. ADMIN = moderate messages, remove, ban. MEMBER = read/send. This is the Telegram model and
  matches how the code is already structured.
- **Delete group:** soft-delete the group and cascade soft-delete memberships. Messages and payment
  transactions are retained for audit and refunds.
- **Ban:** new per-group table, unban supported, blocks rejoin and direct-add in that group only.
- **Fee change:** existing members keep paid access permanently; the new fee applies to new joins
  only; the commission snapshot is refreshed; FREE↔PAID toggling is allowed.
- **Create-modal avatar:** replace the free-text URL field with a real file upload.
- **Member list:** full roster for authorized callers, access gate only — no search/paging now.

---

## 2. Caller audit (prerequisite for the access change)

All three callers of `IChatGroupService.GetMembersAsync` were traced before changing authorization.

| Caller | Impact of adding a membership gate |
|--------|-----------------------------------|
| `ChatGroupController.cs:81` | The REST endpoint itself — this is the intended gate. |
| **`ChatGroupHub.cs:35`** | **BREAKAGE.** The hub calls `GetMembersAsync` *as* its membership check and throws `HubException` when the caller is absent from the returned list. Once the method enforces membership it returns 403 for everyone, and the hub would reject **all** room joins including legitimate members. Must be refactored first. |
| `Chat.razor:917` → `LoadGroupMembersAsync` | **Safe.** The sole call site is `Chat.razor:598`, guarded by `if (thread.IsJoined)`. The existing member UI never requests a roster it is not entitled to. |

`ChatGroupMembershipTests.Creator_CanSee_Members:269` uses the OWNER token, so it continues to pass.

### Unrelated defect found during the audit

`ActivateThreadAsync` (`Chat.razor:592-608`) returns early for a non-joined group **without clearing
`groupMembers`**, and `ToggleInfo` (`:1341`) only flips a boolean — it never refetches. After viewing
group A's roster, opening group B's info panel renders **group A's members**. The server-side gate
closes the API hole; a client-side clear closes the display hole.

---

## 3. Architecture constraints

Established from the codebase and binding on this work:

- **Database-first, no EF migrations.** Entities are scaffolded; DDL is applied by idempotent
  SQL seeders at API startup (`Api/Program.cs:92-99`). `ChatMessageFeaturesDatabaseSeeder.cs:9-14`
  documents the convention.
- **Controllers live in `CommunityLink.Domain`**, registered as an application part in
  `Api/Program.cs:17-19`. Service-layer authorization is the group-chat convention; group endpoints
  use only `[Authorize]` (`PremiumGroupChatAuthorizationTests.cs:233`).
- **All URLs must be absolute.** The Blazor App has a different wwwroot from the API, so a relative
  `/uploads/...` 404s in the browser (`PublicUrlBuilder.cs:8-12`). `ApiBase:PublicBaseUrl` is
  configured in `Api/appsettings.json`.
- **`IPublicUrlBuilder` is a singleton** (`FeatureManager.cs:55`); `IChatAttachmentStorage` is
  registered as a singleton alongside it (`:65`).
- **No base64 data URIs exist anywhere in C# today.** The `NVARCHAR(MAX)` widening of
  `TblChatGroup.AvatarUrl` in `ChatMessageFeaturesDatabaseSeeder.cs:32-35` is vestigial and is left
  in place — narrowing it is a breaking schema change for existing rows and is out of scope.
- **Tests use in-memory EF** (`CommunityApiFactory.cs:20-24`), which does not enforce `rowversion`.
  `UnifiedChatReadTests.cs:208-210` already documents this limitation; new tests will not claim
  concurrency coverage they cannot demonstrate.

---

## 4. Part 1 — Member list restricted to active members

**Business rule:** only active Chat Group members may access the full member list. Non-members may
still view public group information and member count.

### 4.1 Service interface

`CommunityLink.Domain/Features/ChatGroup/IChatGroupService.cs` — add:

```csharp
Task<Result> IsActiveMemberAsync(int chatGroupId, CancellationToken cancellationToken = default);
Task<Result<bool>> IsActiveMemberAsync(int chatGroupId, int userId, CancellationToken cancellationToken = default);
```

### 4.2 Gate the roster

`ChatGroupService.cs:330` — after the existing `chatGroupExists` check, insert the membership gate.
Ordering matters: the group-existence check stays **first** so a missing group returns 404 rather
than 403.

### 4.3 Refactor the hub (must land with the gate)

`ChatGroupHub.cs:35` — replace the roster fetch with `IsActiveMemberAsync(chatGroupId)`, **preserving
the `HubException` throw** so `SignalR_NonMember_CannotJoinHubGroup`
(`ChatGroupSignalRTests.cs:154`) keeps passing. This also stops loading the entire roster on every
room join.

### 4.4 Client

- `Chat.razor:603` — add `groupMembers = Array.Empty<ChatGroupMemberModel>();` to the non-joined
  branch of `ActivateThreadAsync`.
- `ChatThreadInfoPanel.razor:92` — change the member-list section condition from `Thread.IsGroup`
  to `Thread.IsGroup && Thread.IsJoined`.

### 4.5 Deliberately unchanged

`GetChatGroupsAsync`, `GetChatGroupByIdAsync` and `GetMyChatGroupsAsync` keep projecting
`MemberCount`, `IsJoined` and `UserRole`. The Discover drawer, the group directory and group detail
therefore continue to work unchanged for non-members — the public surface is preserved exactly as
required.

---

## 5. Part 2 — Public Chat Group image storage

### 5.1 Policy

New `CommunityLink.Shared/Features/ChatGroup/ChatGroupImagePolicy.cs`, mirroring
`ChatAttachmentPolicy`:

- `MaxBytes = 5 MB`
- `AllowedExtensions = [.jpg, .jpeg, .png, .webp]`
- `AcceptAttribute` for the client file picker
- `GetExtension` helper

### 5.2 Storage abstraction

New `CommunityLink.Domain/Features/ChatGroup/ChatGroupImageStorage.cs` with `IChatGroupImageStorage`
+ implementation, using the same interface/implementation split as `IChatAttachmentStorage`.

**Validation order — size, then extension, then MIME:**

- `declaredLength <= 0` → reject; `> MaxBytes` → reject
- extension must appear in the allow-list. **MIME alone is never trusted**; browsers misreport
  content types on Windows, so the extension is authoritative and MIME is a second gate
- `contentType` must begin with `image/`

**Filename and path safety:**

- Stored name is `chatgroup_{chatGroupId}_{Guid.NewGuid():N}{ext}`. The original filename is used
  **only** to look up a validated extension, never as a path component.
- The incoming name passes through `Path.GetFileName()` before anything else.
- The base directory is built with `Path.GetFullPath` and re-checked with `StartsWith(baseDir)` as
  defense-in-depth against traversal.
- Written with `await using` + `FileMode.CreateNew` and an 80 KB buffer, matching
  `ChatAttachmentStorage.cs:60-68` — the file is streamed, never fully buffered.
- Returns `publicUrlBuilder.Build($"/uploads/chat-groups/{storedName}")` → absolute.
- **Only the public URL is persisted to `TblChatGroup.AvatarUrl`. No base64 data URIs.**

Files are stored under `wwwroot/uploads/chat-groups/`.

Registered in `FeatureManager.cs` next to `IChatAttachmentStorage` as a singleton; it depends only
on the singleton `IPublicUrlBuilder`.

### 5.3 Service

`UpdateImageAsync(chatGroupId, stream, fileName, contentType, length, ct)` on `ChatGroupService`:

- OWNER-only
- save through the storage abstraction
- set `AvatarUrl` to the returned URL, stamp `UpdatedAt` / `UpdatedBy`
- return `Result<ChatGroupModel>` so callers receive the authoritative URL and no bespoke response
  DTO is needed

A private `MapToModelAsync` helper is extracted for the new methods, because the 12-field
projection is currently inlined at `GetChatGroupByIdAsync:209` and `GetChatGroupsAsync:128`.

### 5.4 Endpoint

`ChatGroupController.cs` — `POST api/chat-groups/{chatGroupId:int}/image` with
`[RequestSizeLimit(5 MB + 65536)]` and `[RequestFormLimits]`, matching
`ChatAttachmentsController.cs:21-22`. Kestrel's global 30 MB cap (`Program.cs:31-34`) already covers
this, so no `Program.cs` change is required.

### 5.5 Create-modal flow: create-then-upload

`POST /api/chat-groups/{id}/image` cannot serve group creation — there is no `chatGroupId` yet. A
separate pre-create upload endpoint would let any authenticated user park orphan files on disk, so
the modal instead:

1. holds the picked `IBrowserFile` locally,
2. posts the group with `AvatarUrl = null`,
3. immediately calls the image endpoint on the returned id.

The creator is already `OWNER` (`ChatGroupService.cs:65-75`), so the OWNER-only gate passes.

**Failure handling:** if the image step fails after the group is created, the group still exists. The
modal surfaces a warning toast offering retry rather than failing the whole creation — discarding a
created group to fix an image would be worse.

`CreateChatGroupRequestModel.AvatarUrl` is retained (still `null` from this modal) so the contract is
unchanged for any other caller.

### 5.6 UI changes

- `CreateChatGroupModal.razor:31` — replace the free-text URL field with `InputFile` + create-then-upload.
  Requires `@using Microsoft.AspNetCore.Components.Forms`; the `avatarUrl` state field and its
  `@bind` are removed.
- `ChatGroupApiService.cs` — new multipart image method, mirroring `UserProfileApiService.cs:30-53`.

---

## 6. Part 3 — Service-layer changes for the remaining features

`IChatGroupService.cs` gains eight methods, implemented in `ChatGroupService.cs` on three new private
helpers that collapse logic currently duplicated inline in `PromoteMemberAsync`,
`DemoteMemberAsync` and `RemoveMemberAsync`:

- `GetCallerRoleAsync` — one query, replacing three copies of the
  `.Where(...).Select(m => m.Role).FirstOrDefaultAsync()` block
- `RequireOwnerAsync` / `RequireModeratorAsync` — Result-returning guards
- `IsBannedAsync`

| Method | Caller | Behaviour |
|--------|--------|-----------|
| `UpdateChatGroupAsync` | OWNER | Name / description. Validates length (200 / 1000). |
| `UpdateImageAsync` | OWNER | See Part 2. |
| `AddMembersAsync` | OWNER | Batch `userIds[]`; idempotent on duplicates; rejects self and banned users. |
| `BanMemberAsync` | OWNER+ADMIN | Soft-deletes the membership, writes a `TblChatGroupBan` row, optional reason. |
| `UnbanMemberAsync` | OWNER+ADMIN | Stamps `RevokedAt` / `RevokedBy`. |
| `GetBansAsync` | OWNER+ADMIN | Active bans only. |
| `UpdateFeeAsync` | OWNER | Validates FREE↔PAID rules, refreshes `CommissionPercentageSnapshot`. |
| `DeleteChatGroupAsync` | OWNER | Soft-deletes the group and cascades soft-deletes to memberships. |

### Defects this work must fix

1. **`GetMembersAsync:330`** — see Part 1.
2. **`RemoveMemberAsync:1275`** — an ADMIN can currently remove another ADMIN. The UI hides this at
   `ChatThreadInfoPanel.razor:229` but the service does not enforce it. Add
   `caller == ADMIN && target == ADMIN` → Forbidden. The target-OWNER guard already exists.
3. **`JoinChatGroupAsync:263`** — revives soft-deleted membership rows. This is exactly the rejoin
   path a ban must block; the same check is needed in `JoinPaidChatGroupAsync` (~line 427).
4. **SignalR room residency** — `ChatGroupHub` checks membership only at join time, so a removed or
   banned member stays in the room and keeps receiving broadcasts until they reconnect. They cannot
   send (the service checks), but the leak is real. Addressed by the targeted
   `ChatGroupRemoved` push described in Part 6.

---

## 7. Part 4 — Schema

New table `TblChatGroupBan`:

| Column | Type | Notes |
|--------|------|-------|
| `ChatGroupBanId` | INT IDENTITY | PK |
| `ChatGroupId` | INT NOT NULL | FK → `TblChatGroup` |
| `UserId` | INT NOT NULL | FK → `TblUser` |
| `BannedBy` | INT NULL | FK → `TblUser` |
| `Reason` | NVARCHAR(500) NULL | |
| `BannedAt` | DATETIME2 NOT NULL | |
| `RevokedAt` | DATETIME2 NULL | set on unban |
| `RevokedBy` | INT NULL | set on unban |
| `RowVersion` | ROWVERSION | concurrency stamp |

**Key detail:** a filtered unique index `ON (ChatGroupId, UserId) WHERE RevokedAt IS NULL`. This
permits exactly one *active* ban per user per group while still allowing unban → re-ban.

No new columns are needed on `TblChatGroup` or `TblChatGroupMember` — existing `AvatarUrl`,
`ChatType`, `JoinFeeLinkDrops` and the `IsDeleted` / `DeletedAt` / `DeletedBy` trio cover everything.

**Files:**

- New `CommunityLink.Database/Scripts/20260929_Chat_Group_Management.sql` — the DDL a DBA can apply
  by hand
- New `CommunityLink.Domain/Features/ChatGroup/ChatGroupManagementDatabaseSeeder.cs` — idempotent
  startup seeder, guarding with `if (!db.Database.IsRelational()) return;` and registered in
  `Api/Program.cs:92-99`
- New `CommunityLink.Database/AppDbContextModels/TblChatGroupBan.cs`
- `AppDbContext.Custom.cs` — DbSet and `OnModelCreatingPartial` configuration, following the
  `TblCommunityAuditLog` precedent at lines 15-19

---

## 8. Part 5 — REST endpoints

All on `ChatGroupController.cs` under `api/chat-groups`, `[Authorize]` with service-layer
enforcement per the existing group-chat convention.

| Method | Route | Caller | Feature |
|--------|-------|--------|---------|
| `GET` | `{id}/members` | active member | View members (gated) |
| `POST` | `{id}/members` | OWNER | Add members |
| `DELETE` | `{id}/members/{uid}` | OWNER+ADMIN | Remove member (hardened) |
| `POST` | `{id}/members/{uid}/ban` | OWNER+ADMIN | Ban |
| `DELETE` | `{id}/members/{uid}/ban` | OWNER+ADMIN | Unban |
| `GET` | `{id}/bans` | OWNER+ADMIN | List bans |
| `PUT` | `{id}` | OWNER | Edit info |
| `PUT` | `{id}/fee` | OWNER | Change fee / type |
| `POST` | `{id}/image` | OWNER | Change image |
| `DELETE` | `{id}` | OWNER | Delete group |

Each mutating endpoint broadcasts through the existing `BroadcastAsync` helper
(`ChatGroupController.cs:196-215`), which already swallows SignalR failures so a hub outage cannot
fail a REST write.

---

## 9. Part 6 — Realtime

Three new events. `ChatGroupRemoved` and `ChatGroupDeleted` target **`Clients.User(uid)` rather than
the room**, because the whole point is to reach someone who may no longer be a member.

| Event | Target | Payload |
|-------|--------|---------|
| `ChatGroupUpdated` | room | `(chatGroupId, ChatGroupModel)` |
| `ChatGroupDeleted` | every member's user id | `(chatGroupId)` |
| `ChatGroupRemoved` | the affected user | `(chatGroupId, userId, reason)` — `REMOVED \| BANNED \| DELETED` |

`ChatGroupRemoved` must call `SetSubscribedGroupAsync(null)` client-side to leave the room. That
closes defect #4 from Part 3.

The existing `GroupMemberUpdated` event is reused for add / ban / unban — no new event needed.

Subscriptions are added in `ChatRealtimeService.cs:267-318` alongside the existing handlers.

---

## 10. Part 7 — Blazor UI

1. `ChatGroupApiService.cs` — the new endpoints, plus the multipart image upload mirroring
   `UserProfileApiService.cs:30-53`.
2. `ChatThreadViewModel.cs` — add `WithGroupDetails(...)`. `Title` / `AvatarUrl` / `Description` are
   `init`-only, so an info edit requires copy-on-write rather than field assignment.
3. `ChatThreadInfoPanel.razor` — grows from read-only into the management surface: an owner action
   bar, a per-member Promote / Demote / Remove / Ban control set, a collapsible **Banned** section,
   the fee editor, and a type-to-confirm dialog for group deletion.
4. **New components** — `ChatGroupEditModal.razor`, `ChatGroupFeeModal.razor`,
   `ChatGroupAddMembersModal.razor`. The add-members modal reuses the already-debounced
   `UserProfileApi.SearchUsersAsync` (`Chat.razor:1251-1294`).
5. `Chat.razor` — wire the new callbacks at lines 173-186. On `ChatGroupUpdated` swap the thread via
   `WithGroupDetails`; on `ChatGroupDeleted` / `ChatGroupRemoved` clear `activeThread` and navigate
   away.

---

## 11. Part 8 — Tests

### `ChatGroupMemberVisibilityTests.cs` (new)

Reuses the `GetUserTokenAsync` helper pattern from `ChatGroupPhase2Tests.cs:40-95`.

- non-member → 403
- active MEMBER → 200
- OWNER → 200
- ADMIN → 200
- **banned member → 403** and **removed member → 403** (a soft-deleted membership row fails the
  `!IsDeleted` predicate)
- a non-member still receives 200 and a real `MemberCount` from `GET api/chat-groups/{id}` and from
  the discover list — asserts the public surface is preserved
- a group that does not exist → 404, not 403

### `ChatGroupImageUploadTests.cs` (new)

Follows `ChatAttachmentUploadTests`, including its `BuildUpload` helper (line 90).

- unauthenticated → 401
- non-owner → 403
- jpg / jpeg / png / webp → 200; the URL is absolute, contains `/uploads/chat-groups/`, and **never
  contains `data:`**
- `.exe`, `.gif`, `.svg` → 400
- `image/svg+xml` and `text/html` MIME → 400 even when the filename ends in `.png`
- 5 MB + 1 byte → 400; exactly 5 MB → 200
- missing file → 400
- **filename traversal:** a file named `../../evil.png` must yield a returned URL containing no `..`,
  and the stored name must differ from the original
- the persisted `AvatarUrl` is a short URL (< 200 characters), not a data URI
- `SignalR_NonMember_CannotJoinHubGroup` continues to pass after the hub refactor

**Note on filesystem side effects:** like `ChatAttachmentUploadTests`, the new image tests write real
files during execution. The traversal test asserts on the returned URL rather than on disk layout,
which keeps it robust regardless of the test host's content root.

### `ChatGroupManagementTests.cs` (new, Part 6 features)

- owner edits info; admin and member → 403
- add members: duplicates idempotent, self rejected, **banned user rejected**
- admin can remove a member; **cannot remove the owner; cannot remove another admin**
- ban blocks both `join` and `join-paid`; unban restores rejoin
- a revoked ban is absent from `GET bans`
- fee: PAID→FREE zeroes the fee; FREE→PAID requires fee > 0; the commission snapshot is refreshed
- **existing paid members retain access after a fee change**
- delete: the group is gone from the list, memberships and messages; memberships are soft-deleted
- non-owner delete → 403

---

## 12. Documentation to write during implementation

XML doc comments on `IChatGroupImageStorage` and on the image action of `ChatGroupController`
recording:

- local `wwwroot` disk storage is suitable for the current **single-instance** deployment
- the approach is scoped to **public** Chat Group avatars only — it is **not** to be used for private
  chat attachments or private user files
- it can later be replaced with shared/blob storage without touching `AvatarUrl` consumers, because
  only a URL is ever persisted

---

## 13. Build and verify

```powershell
dotnet build CommunityLink.slnx
dotnet test CommunityLink.Api.Tests\CommunityLink.Api.Tests.csproj
dotnet test CommunityLink.App.Tests\CommunityLink.App.Tests.csproj
```

CSS is Tailwind v3 and is only rebuilt when `node_modules` exists
(`CommunityLink.App.csproj:22-24` silently skips otherwise):

```powershell
cd CommunityLink.App
npm install
npm run build:css
```

---

## 14. Suggested execution order

| Step | Work | Rationale |
|------|------|-----------|
| 1 | Part 1 — member visibility gate + hub refactor | Contains the one breaking change; do it alone so the diff is reviewable and the hub fix is obviously paired with it. |
| 2 | Part 2 — image storage | Self-contained: policy, abstraction, service, endpoint, tests. |
| 3 | Part 7 — schema (`TblChatGroupBan`) | Everything ban-related depends on it. |
| 4 | Part 6 — service methods for the remaining features | With the ban table in place. |
| 5 | Part 5 — REST endpoints | Thin wrappers over Part 6. |
| 6 | Part 6 — realtime events | Depends on the endpoints existing. |
| 7 | Part 8 — Blazor UI | Largest surface; do once the API is proven by tests. |
| 8 | Documentation pass | Reflects final behaviour. |

Steps 1 and 2 are independently shippable and carry the highest security value. Steps 3-8 form the
remainder of the requested feature set.
