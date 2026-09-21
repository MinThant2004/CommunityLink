# COMMUNITY HUB PLATFORM
## Complete Functional Flow Specification

This document provides the end-to-end functional flow, business logic, user journeys, and system rules for the **Community Hub** platform.

---

# 1. System Overview & Core Value Proposition

The **Community Hub** is a connection marketplace and interest-driven community platform. Its primary business objective is to **facilitate and monetize high-value connections** between standard users and verified, high-profile figures (such as Domain Professionals, Industry Leaders, and Public Figures) through structured communities.

### Key Functional Pillars:
1. **Verified Identity Tiers:** Clear visual badges for ⭐ **Domain Professionals** and 🏛️ **Public Figures**.
2. **Hierarchical Community Architecture:** Top-level **Main Communities** containing specialized **Sub-Communities** and **Groups**.
3. **Independent Access Matrix:** Group **Visibility** (`Public` / `Private`) is decoupled from **Join Policy** (`Instant` / `Approval Required`).
4. **Interactive Multi-Filter Feed:** Seamless toggling between `Posts`, `Groups`, and `People` without page reloads.
5. **Direct Engagement Engine:** 1-on-1 private messaging, interactive polls, post interactions, dual rating systems (People & Groups), and real-time notifications.
6. **Dynamic RBAC & Isolated Admin Boundary:** Full runtime permission configuration and dedicated security isolation for platform administrators.

---

# 2. User Roles & Capabilities Matrix

The platform implements **Dynamic Role-Based Access Control (RBAC)** across four primary user personas:

| Capability | Normal User 👤 | Domain Professional ⭐ | Public Figure 🏛️ | Admin 🛡️ |
| :--- | :---: | :---: | :---: | :---: |
| **Browse Public Communities & Feeds** | ✅ | ✅ | ✅ | ✅ |
| **Join Communities (Instant or Request)**| ✅ | ✅ | ✅ | ✅ |
| **Create Main & Sub-Communities** | ✅ | ✅ | ✅ | ✅ |
| **Create Posts & Media Galleries** | ✅ | ✅ | ✅ | ✅ |
| **Create Interactive Polls** | ✅ | ✅ | ✅ | ✅ |
| **1-on-1 Direct Messaging** | ✅ | ✅ | ✅ | ✅ |
| **Rate People & Communities** | ✅ | ✅ | ✅ | ✅ |
| **Verified Profile Badge** | ❌ None | ⭐ Pro Badge | 🏛️ VIP Badge | 🛡️ Admin Badge |
| **Receive & Monetize Public Ratings** | ❌ No | ✅ Yes | ✅ Yes | ❌ No |
| **Platform-Level Moderation & RBAC** | ❌ No | ❌ No | ❌ No | ✅ Yes |
| **Review Identity Verification Queue** | ❌ No | ❌ No | ❌ No | ✅ Yes |

---

# 3. Complete End-to-End Functional Flows

```mermaid
flowchart TD
    %% Global Entry
    Start([User Registration / Login]) --> Home[Home Discovery Dashboard]

    %% Home Dashboard Flow
    subgraph Home Discovery Dashboard
        Home --> Shelf1[🌟 Best Rated People<br/>• Domain Pros ⭐ & Public Figures 🏛️]
        Home --> Shelf2[🚀 Popular Communities & Sub-Communities]
    end

    %% Profile Path
    Shelf1 -->|Click Profile| ProfilePage[👤 User Profile Page<br/>• View Bio, Badges & Rating<br/>• Save Account / Start 1-on-1 Chat]

    %% Community Path
    Shelf2 -->|Click Community| SubCommPage[📂 Sub-Communities Page<br/>• List Sub-Communities & Groups]

    subgraph Sub-Community Actions
        SubCommPage --> SubCard[Sub-Community Card]
        SubCard -->|Click 'Join'| JoinLogic{Join Policy Check}
        JoinLogic -->|Instant| InstantJoin[Active Member]
        JoinLogic -->|Approval Required| RequestJoin[Pending Join Request in Owner Queue]
        SubCard -->|Click 'See Sub-Community'| SubFeed[📰 Sub-Community Feed Page]
    end

    %% Interactive Multi-Select Feed
    subgraph Sub-Community Interactive Feed
        SubFeed --> FilterControl{Multi-Select Filter Buttons}
        
        FilterControl -->|Toggle [ Post ]| PostSection[📝 Posts & Polls Feed<br/>• Rich Text & Media Galleries<br/>• Interactive Poll Voting<br/>• Likes, Comments & Shares]
        
        FilterControl -->|Toggle [ Group ]| GroupSection[👥 Groups List<br/>• Public & Private Groups<br/>• Group Ratings & Member Counts]
        
        FilterControl -->|Toggle [ People ]| PeopleSection[⭐ Verified People Roster<br/>• Filter Domain Pros & Public Figures<br/>• View Ratings & Initiate Chat]
    end

    PeopleSection -->|Click Person| ProfilePage
```

---

## Flow 1: Authentication, Onboarding & Identity Verification

```
[User Signs Up] ──> [Email / Password Auth] ──> [Standard User Account Created]
                                                        │
                      ┌─────────────────────────────────┴─────────────────────────────────┐
                      ▼                                                                   ▼
          [Apply for Domain Pro ⭐]                                            [Apply for Public Figure 🏛️]
                      │                                                                   │
           (Work/Domain Credential)                                            (Gov ID + Official Proof)
                      │                                                                   │
                      └───────────────────────────────┬───────────────────────────────────┘
                                                      ▼
                                       [Admin Review & Audit Log]
                                                      │
                                    ┌─────────────────┴─────────────────┐
                                    ▼                                   ▼
                             [Badge Approved]                    [Badge Rejected]
```

1. **Registration & Login:**
   * User registers with Username, Email, Password, and Display Name.
   * System creates standard record in `TblUser` with `IsVerified = false` and assigns `NORMAL_USER` role via `TblUserRole`.
2. **Identity Verification Application:**
   * User applies for **Domain Professional** (submitting work email, LinkedIn, or portfolio) or **Public Figure** (submitting government identification and public documentation).
3. **Admin Verification Review:**
   * Platform administrators review submissions in the isolated Admin portal.
   * On approval, the system updates `TblUser.IsVerified = true`, attaches the respective role (`DOMAIN_PRO` or `PUBLIC_FIGURE`), and logs the event in `TblAuditLog`.

---

## Flow 2: Home Discovery Dashboard

Upon successful login, the user lands on the **Home Discovery Dashboard**:

1. **🌟 Best-Rated People Shelf:**
   * Displays top-rated **Domain Professionals** and **Public Figures** ranked by `AverageRating` and `RatingCount`.
   * Cards display user avatar, display name, verification badge (⭐ or 🏛️), star rating, and direct action buttons (**View Profile**, **Save Account**).
2. **🚀 Popular Communities Shelf:**
   * Displays trending Main Communities and Sub-Communities ranked by `MemberCount` and `PostCount`.
3. **Navigation Actions:**
   * Clicking a person card routes to **User Profile Page**.
   * Clicking a community card routes to the **Sub-Communities Page**.

---

## Flow 3: Community & Sub-Community Management

```
Main Community (e.g., "Technology")
  │
  ├── ParentCommunityId = NULL
  │
  └── Sub-Community (e.g., "C# & .NET Development")
        │
        ├── ParentCommunityId = [Technology.CommunityId]
        ├── Visibility: [ PUBLIC | PRIVATE ]
        └── JoinPolicy: [ INSTANT | APPROVAL_REQUIRED ]
```

1. **Browsing Sub-Communities:**
   * Selecting a Main Community displays all related Sub-Communities (`ParentCommunityId = MainCommunity.CommunityId`).
2. **Sub-Community Card Actions:**
   * **`See Sub-Community` Button:** Allows the user to open and preview the Sub-Community feed. If `Visibility = PUBLIC`, non-members can view posts; if `PRIVATE`, post content is hidden until membership is active.
   * **`Join Sub-Community` Button:**
     * If `JoinPolicy = INSTANT`: Instantly creates active record in `TblCommunityMember` and increments `MemberCount`.
     * If `JoinPolicy = APPROVAL_REQUIRED`: Creates a record in `TblCommunityJoinRequest` with `Status = PENDING`. The community Owner/Moderator reviews and accepts/rejects the request.

---

## Flow 4: Interactive Sub-Community Multi-Filter Feed

Inside a Sub-Community, users interact with a single dynamic feed controlled by **Multi-Select Filter Buttons** (pill toggles):

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│  SUB-COMMUNITY: C# & .NET                                                       │
│  [  Post  ✓  ]      [  Group  ✓  ]      [  People  ✓  ]    <-- Toggle Buttons   │
└─────────────────────────────────────────────────────────────────────────────────┘
```

### 1. `[ Post ]` View:
* Displays posts published in this Sub-Community.
* **Post Creation:** Author can publish rich text, upload multiple images (`TblPostImage`), or attach an interactive poll.
* **Poll Participation:**
  * Displays question, single/multi-choice options, total vote count, and expiration timer.
  * User casts a vote (`TblPollVote`); system updates `TblPollOption.VoteCount` and caches `TblPoll.TotalVotes` in real time.
* **Social Engagement:**
  * ❤️ **Like:** Toggle like status in `TblPostLike`; updates `LikeCount`.
  * 💬 **Comment:** Add top-level comments or nested replies (`TblComment`).
  * 🔖 **Save Post:** Bookmark post into user's private library (`TblSavedPost`).
  * 🔗 **Share:** Repost to personal profile or another community (`TblPostShare`).

### 2. `[ Group ]` View:
* Displays specialized group rooms operating within the Sub-Community.
* Displays group name, description, privacy badge (`PUBLIC` / `PRIVATE`), member count, and average star rating (`TblCommunityRating`).

### 3. `[ People ]` View:
* Displays verified members active in this Sub-Community.
* Distinguishes ⭐ **Domain Professionals** from 🏛️ **Public Figures** via visual badge styling.
* Displays member average ratings, bios, and direct buttons to **Save Account** or **Send 1-on-1 Chat Message**.

---

## Flow 5: 1-on-1 Direct Chat System

```
[User A clicks 'Message' on User B]
                │
                ▼
   {Check Existing Conversation}
   ├── Exists: Retrieve ConversationId
   └── None: Create TblConversation (UserOneId, UserTwoId)
                │
                ▼
[Send Message] ──> Insert TblChatMessage ──> Dispatch Real-Time Alert to User B
                                         ──> Update TblConversation.LastMessageAt
```

1. **Conversation Initiation:**
   * A user clicks **"Message"** on another user's profile card.
   * System checks `TblConversation` for an existing thread between the two users (order-independent: `(UserOneId, UserTwoId)` or `(UserTwoId, UserOneId)`).
   * If none exists, a new `TblConversation` record is created.
2. **Messaging & Attachments:**
   * Users send text messages with optional media attachments (`AttachmentUrl`).
   * System saves record to `TblChatMessage`, updates `TblConversation.LastMessagePreview` and `LastMessageAt`.
3. **Read Receipts:**
   * When the recipient opens the chat, `TblChatMessage.IsRead` is marked `true` and `ReadAt` timestamp is recorded.

---

## Flow 6: Rating & Reputation Engine

```
                                  [RATING ENGINE]
                                         │
                 ┌───────────────────────┴───────────────────────┐
                 ▼                                               ▼
     [Rate a Person (User)]                          [Rate a Community]
                 │                                               │
    Insert into TblUserRating                      Insert into TblCommunityRating
                 │                                               │
   Recalculate TblUser.AverageRating              Recalculate TblCommunity.AverageRating
       & TblUser.RatingCount                          & TblCommunity.RatingCount
```

1. **People Rating:**
   * Users can submit a 1–5 star rating and review text for Domain Professionals and Public Figures.
   * System stores the record in `TblUserRating` and automatically updates the cached `AverageRating` and `RatingCount` on `TblUser`.
2. **Community Rating:**
   * Active members can submit a 1–5 star rating and review for any Community or Group.
   * System stores the record in `TblCommunityRating` and automatically updates `AverageRating` and `RatingCount` on `TblCommunity`.

---

## Flow 7: Real-Time Notification Engine

1. **Trigger Events:**
   * Notifications are automatically generated for:
     * Post Liked / Commented on (`LIKE`, `COMMENT`)
     * Group Join Request received by owner (`JOIN_REQUEST`)
     * Group Join Request accepted (`JOIN_ACCEPTED`)
     * Incoming 1-on-1 message (`NEW_MESSAGE`)
     * Rating/Review received (`RATING_RECEIVED`)
     * Platform Announcements (`SYSTEM`)
2. **Delivery & Deep Linking:**
   * Record inserted into `TblNotification` with `TargetEntityName` and `TargetEntityId`.
   * Real-time push alert dispatched to the recipient.
   * Clicking the notification opens the target post, group, or chat thread directly.

---

## Flow 8: Security, Audit Logging & Concurrency Control

1. **Strict Admin/User Boundary:**
   * `TblAdmin` and `TblUser` maintain completely separated credentials, preventing privilege escalation.
2. **Audit Trail (`TblAuditLog`):**
   * Every insert, update, soft-delete, and verification approval records `ActorType`, `ActorId`, `EntityName`, `OldValues` (JSON), `NewValues` (JSON), `IpAddress`, and `UserAgent`.
3. **Optimistic Concurrency & Soft Delete:**
   * All 26 database tables implement `RowVersion` for concurrency control and `IsDeleted`, `DeletedAt`, `DeletedBy` for data recovery and audit compliance.

---

# 4. Summary Lifecycle Diagram

$$\begin{aligned}
\text{Sign Up} &\longrightarrow \text{Verification (Optional Pro/VIP Badge)} \\
&\longrightarrow \text{Home Discovery (Best Rated People \& Top Communities)} \\
&\longrightarrow \text{Explore Sub-Communities (Instant Join / Request Approval)} \\
&\longrightarrow \text{Sub-Community Feed (Filter by Posts, Groups, People)} \\
&\longrightarrow \text{Engage (Polls, Likes, Comments, Shares, 1-on-1 Chat, Star Ratings)}
\end{aligned}$$
