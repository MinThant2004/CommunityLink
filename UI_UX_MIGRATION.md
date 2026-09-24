# Implementation Plan: System-Wide Light Mode UI/UX Migration (UI_UX_DESIGN_SYSTEM.md)

Migrate the entire CommunityLink platform from its legacy dark slate aesthetic to the unified **Executive Minimalist Light Mode** design system documented in [UI_UX_DESIGN_SYSTEM.md](UI_UX_DESIGN_SYSTEM.md). 

This migration standardizes all application layouts, navigation components, core collaborative modules, admin governance screens, and authentication portals into a cohesive, high-contrast, institutional experience.

---

## User Review Required

> [!IMPORTANT]
> **Scope Confirmation:** Per user instruction, this phase focuses exclusively on **Light Mode** across all screens. Dark mode tokens and switching mechanisms will be implemented in a subsequent phase.

> [!NOTE]
> The **Home page** ([Home.razor](CommunityLink.App/Components/Pages/Home.razor)) and **User Profile** ([UserProfile.razor](CommunityLink.App/Components/Pages/UserProfile.razor)) were already updated to this standard and will serve as the visual reference benchmarks for all other screens.

---

## Proposed Changes

```
+------------------------------------------------------------------------+
|                        MIGRATION ROADMAP                               |
|  PHASE 1: Foundation & App Shell (App.razor, MainLayout, NavMenu)      |
|  PHASE 2: Core Collaboration (Communities, Groups, GroupDetail)        |
|  PHASE 3: Feed, Polls & Chat (PostUpload, Polls, Chat)                 |
|  PHASE 4: User Dashboard & Security (UserDashboard, RolePermissions)   |
|  PHASE 5: Admin Governance (AdminDashboard, Users, Hubs, AuditLogs)    |
|  PHASE 6: Authentication & Guest Entry (Login, Register, AccessDenied) |
+------------------------------------------------------------------------+
```

---

### Phase 1: Foundation & Application Shell (Global Layouts)

Transform the global layout shell so that all page content is framed within an executive white and Arctic Haze canvas.

#### [MODIFY] App.razor
- Remove `class="dark"` from `<html>`.
- Change `<body>` class from `bg-[#0B1326] text-[#F8FAFC]` to `bg-[#F8FAFC] text-[#0A1B2E] antialiased`.
- Inject Google Fonts `<link>` (Manrope, Inter, JetBrains Mono) globally in `<head>`.

#### [MODIFY] MainLayout.razor
- Convert outer shell from `bg-[#0b1326]` to `bg-[#F8FAFC] text-[#0A1B2E]`.
- Convert sidebar wrapper from `bg-[#0f172a] border-slate-800` to `bg-white border-r border-[#E2E8F0] shadow-sm`.
- Convert top header bar to white (`bg-white/95 border-b border-[#E2E8F0] backdrop-blur-md`).
- Update top bar breadcrumb, status badge (`Live SignalR Synced`), and user profile pill to Executive Minimalist styling.
- Set `<main>` background to `#F8FAFC`.

#### [MODIFY] NavMenu.razor
- Modernize brand logo: Deep Navy mark with `Community Link` in Manrope bold.
- Update `NavLink` styling:
  - Inactive: text `#557392`, hover `#F1F5F9` background with `#0A1B2E` text.
  - Active: solid Deep Navy `#0A1B2E` background with pure white text (`#FFFFFF`) and `0.25rem` radius.
- Admin section divider: clean `#E2E8F0` divider with JetBrains Mono uppercase header `ADMINISTRATION`.
- Profile footer card: clean white recessed card with user initials, role badge, and link to `/profile`.

---

### Phase 2: Core Collaboration (Communities & Groups)

#### [MODIFY] Communities.razor
- Replace dark canvas (`bg-[#0B1326]`) with `#F8FAFC`.
- Modernize Search Bar: oval input with `#F1F5F9` fill, `#CBD5E1` border, and focus halo.
- Primary Action buttons: Deep Navy `#0A1B2E` for *"Create Community"*, Secondary outline for *"Create Sub-community"*.
- Community cards: Pure white `#FFFFFF` surface, hairline `#E2E8F0` border, Manrope headline, Inter description, JetBrains Mono member counters (`👥 1,940 members`).
- Sub-community hierarchy accordion: Clean nested panel styling (`#F1F5F9`) with tier pips.
- Create/Edit side drawer form: White background, `#CBD5E1` input borders, clean validation alerts.

#### [MODIFY] Groups.razor
- Convert top banner and filters to white cards with `#E2E8F0` borders.
- Sub-community filter dropdown: executive input styling (`bg-white border-[#CBD5E1] text-[#0A1B2E]`).
- Group room cards: white `#FFFFFF` cards, Level 1 shadow, Level 2 on hover, category chips, member count, *"Join Room"* button.
- Create Group modal: white card with Deep Navy submit button.

#### [MODIFY] GroupDetail.razor
- Group hero banner & header: white card with cover backdrop, group title in Manrope bold, tier tag.
- Tab bar (Discussions, Live Chat, Members, Files): clean underline or pill tabs.
- Group chat stream & resource lists: light mode messaging bubbles, file attachment cards.

---

### Phase 3: Feed, Live Polls & Real-Time Chat

#### [MODIFY] Polls.razor
- Replace dark slate cards (`bg-[#1E293B]`) with `#FFFFFF` cards with `#E2E8F0` borders.
- Live poll cards: question in Manrope 16px bold, creator chip in JetBrains Mono.
- Voting option progress bars: track in `#E1E8F0`, fill in Deep Navy `#0A1B2E`, vote count in JetBrains Mono.
- Create Poll modal: white dialog with clean option inputs.

#### [MODIFY] PostUpload.razor (Public Feed)
- Broadcast feed stream: convert post cards to white surfaces with `#E2E8F0` hairline dividers.
- Broadcast composer: halo-focus textarea, target hub select dropdown, Deep Navy broadcast button.
- Post engagement actions: like count, comment count, and share actions with subtle `#557392` text.

#### [MODIFY] Chat.razor
- Messenger dual-pane layout:
  - Left conversation rail: white card with search, contact items, online indicator pips (`#10B981`), unread count badges.
  - Active conversation header: contact avatar, display name, online status.
  - Message bubble stream:
    - Received bubbles: Arctic Haze `#F1F5F9` with `#0A1B2E` text and `#E2E8F0` border.
    - Sent bubbles: Deep Navy `#0A1B2E` with pure white text (`#FFFFFF`).
  - Chat input composer: white input, attachment icon, send button.

---

### Phase 4: User Dashboard & Role Governance

#### [MODIFY] UserDashboard.razor
- Convert 4-item KPI metric strip to white cards with Manrope bold numbers and JetBrains Mono labels.
- Recent activity timeline: hairline vertical connector with status pips.
- Quick navigation shortcuts: secondary buttons with hover lift.

#### [MODIFY] RolePermissions.razor
- Role privilege cards: white cards, `#E2E8F0` dividers, permission checkboxes with `#0A1B2E` checked state.
- Save permissions CTA: Deep Navy primary button.

#### [MODIFY] AccessDenied.razor
- Clean executive security advisory card: white card with crimson accent tag and return home CTA.

---

### Phase 5: Administration Governance Portal

#### [MODIFY] AdminDashboard.razor
- Executive system stats: total users, active hubs, SLA breach monitors, and storage metrics in analytical cards.
- Quick admin action grid: white cards with clean icons and navigation links.

#### [MODIFY] AdminUsers.razor
- User directory data table:
  - Header row: Arctic Haze `#F1F5F9` background, `JetBrains Mono` uppercase labels in Dusk Blue (`#7A8CA6`).
  - Rows: white background, `#E2E8F0` hairline bottom border, hover transition to `#F8FAFC`.
  - Monospaced user IDs, role badges, action buttons (Edit, Ban, Reset).

#### [MODIFY] AdminCommunities.razor
- Hub moderation data table: review submitted communities, approve/reject action buttons, category tags.

#### [MODIFY] AdminAuditLogs.razor
- Cryptographic timeline & log viewer: monospaced timestamp, actor, action code, and JSON payload viewer with syntax styling.

---

### Phase 6: Authentication & Guest Entry Flows

#### [MODIFY] UserLogin.razor & UserRegister.razor
- Authentication card: centered white card `#FFFFFF`, hairline border `#E2E8F0`, soft shadow.
- Brand header: Community Link logo with `VERIFIED NODE PROTOCOL` tag.
- Form inputs: white background, `#CBD5E1` border, halo-focus state.
- Primary button: Deep Navy `#0A1B2E` with white text.

#### [MODIFY] AdminLogin.razor & AdminRegister.razor
- Secured operator login with executive minimalist styling and security badges.

#### [MODIFY] Guest.razor
- Guest landing page hero: harmonize lighting and typography with Executive Minimalist tokens.

---

## Verification Plan

### Automated Verification
- Run project compilation checks:
  ```powershell
  dotnet build "CommunityLink.App/CommunityLink.App.csproj" -t:CoreCompile
  ```
- Ensure zero errors (`0 Error(s)`) and zero critical warnings.

### Manual Visual Verification
1. **Global Shell Test:** Navigate across `/home`, `/communities`, `/groups`, `/polls`, `/chat`, `/profile`, `/admin/dashboard`, and verify that the sidebar, header, and canvas maintain unified `#F8FAFC` / `#FFFFFF` / `#0A1B2E` colors without jarring dark backgrounds.
2. **Interactive States Test:** Hover over cards, click buttons, inspect input focus halos, and test modals (Create Group, Create Poll, Create Community, Edit Profile).
3. **Typography & Contrast Test:** Confirm all headers are rendered in Manrope, body copy in Inter, and counters/chips in JetBrains Mono. Verify that no text drops below WCAG AA contrast standards against `#FFFFFF` or `#F8FAFC`.
