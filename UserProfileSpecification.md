# User Profile Module: Developer Technical Specification

This document provides the complete, implementation-ready specification for the **User Profile Module** and the **Sidebar Navigation Integration**. A developer can follow this guide to implement the frontend sidebar menu, profile page UI/UX, backend APIs, validation pipelines, and database queries.

---

## 1. Feature Overview & UI Structure

### A. Sidebar Navigation Integration
Add a permanent **"Profile"** navigation item in the main application sidebar:
* **Icon:** User Avatar thumbnail or standard Profile Icon.
* **Label:** `"Profile"` (or display the user's name).
* **Navigation Target:** Routes to `/profile` (the authenticated user's own profile).
* **Sidebar Profile Card (Footer / Header):**
  * Displays user avatar, display name, and their verified identity badge (⭐ **Domain Pro** / 🏛️ **Public Figure** / 👤 **Member**).

```
┌──────────────────────────────┐
│  🌐 COMMUNITY HUB            │
│                              │
│  🏠  Home                    │
│  📂  Communities             │
│  💬  Messages (Chat)         │
│  🔔  Notifications           │
│  👤  Profile  <-- [NEW MENU] │
│  ⚙️  Settings                │
│                              │
│ ───────────────────────────  │
│  [Avatar]  Sarah Jenkins ⭐  │
│  @sarah_j • Domain Pro       │
└──────────────────────────────┘
```

---

### B. Dual Profile View Modes

The Profile Page operates in two distinct modes based on whether the viewer is the account owner:

```
                  ┌───────────────────────────────────────────────────────────┐
                  │                 USER PROFILE PAGE                         │
                  ├─────────────────────────────┬─────────────────────────────┤
                  │     OWNER VIEW (/profile)   │   VISITOR VIEW (/u/:user)   │
                  ├─────────────────────────────┼─────────────────────────────┤
                  │ • [Edit Profile] Button     │ • [Save Account / Bookmark] │
                  │ • Tab: My Posts             │ • [Send 1-on-1 Message]     │
                  │ • Tab: Saved Posts / Items  │ • [Rate User (1–5 Stars)]   │
                  │ • Tab: Saved Accounts       │ • Tab: Public Posts         │
                  │ • Tab: Joined Communities   │ • Tab: Public Communities   │
                  │ • Tab: Reviews Received     │ • Tab: Public Reviews       │
                  └─────────────────────────────┴─────────────────────────────┘
```

---

## 2. Profile Creation & Update DTOs

### 1. Profile Update Request Payload (`UpdateUserProfileRequestDto`)

| Field Name | Type | Required? | Validation Rules & Constraints |
| :--- | :--- | :---: | :--- |
| `DisplayName` | `String` | **Yes** | • Min: 2 characters, Max: 100 characters.<br>• Cannot be blank or whitespace only. |
| `UserName` | `String` | **Yes** | • Min: 3 characters, Max: 50 characters.<br>• Lowercase alphanumeric and underscores only (`^[a-zA-Z0-9_]+$`).<br>• Must be globally unique in `TblUser`. |
| `Bio` | `String` | No | • Max: 500 characters.<br>• Plain text or brief markdown summary. |
| `AvatarFile` / `AvatarUrl` | `File` / `String` | No | • Allowed formats: `.jpg`, `.jpeg`, `.png`, `.webp`.<br>• Max file size: 5 MB.<br>• Uploads to CDN/Storage and stores the URL in `TblUser.AvatarUrl`. |

---

## 3. Database Entities & Relationships Used

```
                ┌──────────────────────────────────────────────┐
                │                   TblUser                    │
                │  UserId, UserName, DisplayName, AvatarUrl,   │
                │  Bio, IsVerified, AverageRating, RatingCount │
                └──────────────────────┬───────────────────────┘
                                       │
         ┌─────────────────────────────┼─────────────────────────────┐
         ▼                             ▼                             ▼
┌──────────────────┐          ┌──────────────────┐          ┌──────────────────┐
│   TblUserRole    │          │  TblSavedAccount │          │  TblUserRating   │
│  (Identity Badge:│          │ (Who saved this  │          │ (1-5 Star Reviews│
│   Pro / VIP)     │          │  user's account) │          │  & Feedback)     │
└──────────────────┘          └──────────────────┘          └──────────────────┘
```

---

## 4. Business Validation & Pre-Conditions

1. **Authentication:**
   * Any request to view or edit the authenticated user's profile requires a valid JWT token (`CurrentUserId`).
2. **Username Uniqueness:**
   * When updating `UserName`, query `TblUser` to ensure the new username is not already claimed by another user (`UserId != CurrentUserId`).
3. **Authorization & Privacy Rules:**
   * **Editing:** Users can ONLY update their own profile (`UserId == CurrentUserId`). Editing another user's profile returns `403 Forbidden`.
   * **Saved Tab Protection:** The "Saved Posts" and "Saved Accounts" tabs are strictly private and only visible to the profile owner.
4. **Rating Eligibility (Visitor View):**
   * Users cannot rate their own profile (`RaterUserId != TargetUserId`).
   * A user can only submit one rating per target user (subsequent ratings update the existing review).

---

## 5. API Contracts

### Endpoint 1: Get Current Logged-In User Profile (Owner View)
```http
GET /api/users/me
Authorization: Bearer <jwt_token>
```

#### Response Example (`200 OK`)
```json
{
  "success": true,
  "data": {
    "userId": "99f85f64-5717-4562-b3fc-2c963f66af11",
    "userName": "alex_morgan",
    "email": "alex.morgan@domain.com",
    "displayName": "Alex Morgan",
    "avatarUrl": "https://cdn.communityhub.com/avatars/alex.jpg",
    "bio": "Senior Cloud Solutions Architect & .NET Open Source Contributor. Passionate about distributed systems.",
    "isVerified": true,
    "role": {
      "roleCode": "DOMAIN_PRO",
      "roleName": "Domain Professional",
      "badge": "STAR"
    },
    "metrics": {
      "averageRating": 4.92,
      "ratingCount": 38,
      "joinedCommunitiesCount": 12,
      "savedAccountsCount": 45,
      "postsCount": 29
    },
    "createdAt": "2026-01-10T10:00:00Z"
  }
}
```

---

### Endpoint 2: Get Public User Profile by Username or ID (Visitor View)
```http
GET /api/users/{userNameOrId}
Authorization: Bearer <jwt_token> (Optional/Required for relationship status)
```

#### Response Example (`200 OK`)
```json
{
  "success": true,
  "data": {
    "userId": "99f85f64-5717-4562-b3fc-2c963f66af11",
    "userName": "alex_morgan",
    "displayName": "Alex Morgan",
    "avatarUrl": "https://cdn.communityhub.com/avatars/alex.jpg",
    "bio": "Senior Cloud Solutions Architect & .NET Open Source Contributor.",
    "isVerified": true,
    "role": {
      "roleCode": "DOMAIN_PRO",
      "roleName": "Domain Professional",
      "badge": "STAR"
    },
    "metrics": {
      "averageRating": 4.92,
      "ratingCount": 38,
      "joinedCommunitiesCount": 12,
      "postsCount": 29
    },
    "relationship": {
      "isSelf": false,
      "isSavedByMe": true,
      "canMessage": true,
      "hasRated": true,
      "myRatingScore": 5
    },
    "createdAt": "2026-01-10T10:00:00Z"
  }
}
```

---

### Endpoint 3: Update Profile Information
```http
PUT /api/users/me
Content-Type: application/json
Authorization: Bearer <jwt_token>
```

#### Request Payload
```json
{
  "displayName": "Alex Morgan",
  "userName": "alex_morgan",
  "bio": "Senior Cloud Architect. Specialized in High-Performance .NET and Microservices."
}
```

---

### Endpoint 4: Upload Profile Avatar
```http
POST /api/users/me/avatar
Content-Type: multipart/form-data
Authorization: Bearer <jwt_token>
```

#### Form Data
* `avatar`: `[Image File: .jpg / .png / .webp, max 5MB]`

#### Response Example (`200 OK`)
```json
{
  "success": true,
  "message": "Avatar updated successfully.",
  "data": {
    "avatarUrl": "https://cdn.communityhub.com/avatars/alex_178954.png"
  }
}
```

---

## 6. HTTP Error Handling Matrix

| HTTP Status | Error Code | Trigger Condition | Response Message Example |
| :---: | :--- | :--- | :--- |
| `400` | `VALIDATION_ERROR` | Display name < 2 chars, invalid characters in username. | `"Username can only contain letters, numbers, and underscores."` |
| `401` | `UNAUTHORIZED` | Missing or expired auth token. | `"Authentication token is required."` |
| `403` | `FORBIDDEN` | Attempting to update another user's profile. | `"You can only update your own profile."` |
| `404` | `USER_NOT_FOUND` | Public username or ID not found. | `"User '@unknown' does not exist."` |
| `409` | `USERNAME_TAKEN` | Updated username is already used by another active user. | `"The username 'alex_morgan' is already taken."` |
| `413` | `AVATAR_TOO_LARGE` | Avatar file size exceeds 5 MB. | `"Avatar image cannot exceed 5 MB."` |
| `415` | `UNSUPPORTED_MEDIA` | Uploaded file is not `.jpg`, `.png`, or `.webp`. | `"Only JPEG, PNG, and WebP images are allowed."` |
| `500` | `SERVER_ERROR` | Database or CDN storage failure. | `"An unexpected error occurred while updating profile."` |

---

## 7. Sample C# Implementation Reference

### FluentValidation Validator
```csharp
public class UpdateUserProfileValidator : AbstractValidator<UpdateUserProfileRequestDto>
{
    public UpdateUserProfileValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display Name is required.")
            .Length(2, 100).WithMessage("Display Name must be between 2 and 100 characters.");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required.")
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters.")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores.");

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio cannot exceed 500 characters.");
    }
}
```

### Service Method (Pseudocode)
```csharp
public async Task<UserProfileResponseDto> UpdateProfileAsync(UpdateUserProfileRequestDto dto, Guid currentUserId)
{
    var user = await _dbContext.TblUsers
        .FirstOrDefaultAsync(u => u.UserId == currentUserId && !u.IsDeleted);

    if (user == null)
        throw new NotFoundException("User not found.");

    // Check username uniqueness if changed
    string normalizedNewUserName = dto.UserName.Trim().ToUpperInvariant();
    if (user.NormalizedUserName != normalizedNewUserName)
    {
        bool isTaken = await _dbContext.TblUsers
            .AnyAsync(u => u.NormalizedUserName == normalizedNewUserName && u.UserId != currentUserId && !u.IsDeleted);

        if (isTaken)
            throw new ConflictException($"Username '{dto.UserName}' is already taken.");

        user.UserName = dto.UserName.Trim();
        user.NormalizedUserName = normalizedNewUserName;
    }

    user.DisplayName = dto.DisplayName.Trim();
    user.Bio = dto.Bio?.Trim();
    user.UpdatedAt = DateTime.UtcNow;
    user.UpdatedBy = currentUserId;

    await _dbContext.SaveChangesAsync();

    return MapToProfileDto(user);
}
```
