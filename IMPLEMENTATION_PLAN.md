# CommunityLink - Technical Framework Implementation Plan

> **Objective**: Complete the technical setup of **CommunityLink** to achieve 100% compliance with `TECHNICAL_FRAMEWORK_BLUEPRINT.md`. This plan outlines the exact files, code patterns, layers, and configuration required to transform the scaffolded database and solution into a fully operational, secure, 5-layer enterprise system.

---

## 1. Executive Summary & Gap Analysis

### Current Status
- [x] Solution created (`CommunityLink.slnx`) with 5 core projects + 2 test projects (.NET 10.0 / C# 14).
- [x] Project references configured according to layer dependencies.
- [x] Entity Framework Core 10 `AppDbContext` and 27 domain entities scaffolded in `CommunityLink.Database`.
- [x] Basic NuGet packages installed across all projects.

### What Needs to Be Added
| Layer | Missing / Required Components |
|---|---|
| **1. `CommunityLink.Shared`** | `PagedResult.cs`, `BaseController.cs`, `CustomSettingModel.cs`, `ChangeRecord.cs`, `PermissionCatalog.cs`, Feature DTO models (Auth, Community, Post, Poll, Chat, Admin). |
| **2. `CommunityLink.Domain`** | `FeatureManager.cs` (DI root with InMemory fallback), `CurrentUserContext.cs`, `TokenIssuer.cs`, `PermissionEvaluator.cs`, `PermissionAuthorization.cs`, `AuthenticationService.cs`, Domain Feature slices. |
| **3. `CommunityLink.Api`** | `Middlewares/ExceptionHandlingMiddleware.cs`, `Middlewares/ApiRequestLoggingMiddleware.cs`, `Middlewares/PasswordChangeRequirementMiddleware.cs`, `Program.cs` full pipeline & JWT auth configuration. |
| **4. `CommunityLink.App`** | Cookie Auth configuration (`Ace.Community.Auth`), `HttpClientFactory` & typed `ApiService` clients, Tailwind CSS npm pipeline & `app.css`, `MainLayout.razor`, `ToastService.cs`, UI Razor pages. |
| **5. Testing** | `CommunityLink.Api.Tests` (`CtsApiFactory.cs`, RBAC integration tests), `CommunityLink.App.Tests` (Route & Render tests). |

---

## 2. Phase 1: Shared Primitives & Contracts (`CommunityLink.Shared`)

### 1.1. Base Result Pattern (`PagedResult.cs`)
* **Path**: `CommunityLink.Shared/PagedResult.cs`
* **Purpose**: Functional wrapper for paginated grid/list responses across all APIs.
```csharp
namespace CommunityLink.Shared;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
```

### 1.2. Base Controller Translation (`BaseController.cs`)
* **Path**: `CommunityLink.Shared/BaseController.cs`
* **Purpose**: Translates `Result<T>` directly into standard HTTP response codes (200, 400, 401, 403, 404, 409, 500).
```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommunityLink.Shared;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected IActionResult ToActionResult<T>(Result<T> result) => result.Status switch
    {
        ResultStatus.Ok => Ok(result),
        ResultStatus.BadRequest => BadRequest(result),
        ResultStatus.Unauthorized => Unauthorized(result),
        ResultStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result),
        ResultStatus.NotFound => NotFound(result),
        ResultStatus.Conflict => Conflict(result),
        _ => StatusCode(StatusCodes.Status500InternalServerError, result)
    };
}
```

### 1.3. Strongly-Typed Settings Schema (`CustomSettingModel.cs`)
* **Path**: `CommunityLink.Shared/CustomSettingModel.cs`
* **Purpose**: Multi-environment configuration models for Database, JWT, and Security.

### 1.4. Permission Catalog (`Security/PermissionCatalog.cs`)
* **Path**: `CommunityLink.Shared/Security/PermissionCatalog.cs`
* **Purpose**: Single Source of Truth for all RBAC permission strings and default role bindings.
```csharp
namespace CommunityLink.Shared.Security;

public sealed record PermissionDefinition(string Code, string Name, string Module, string? RoutePath, bool AdminDefault, bool MemberDefault);

public static class PermissionCatalog
{
    public const string PolicyPrefix = "Permission:";

    // Communities
    public const string CommunityView = "COMMUNITY.VIEW";
    public const string CommunityCreate = "COMMUNITY.CREATE";
    public const string CommunityManage = "COMMUNITY.MANAGE";

    // Posts & Feed
    public const string PostView = "POST.VIEW";
    public const string PostCreate = "POST.CREATE";
    public const string PostDelete = "POST.DELETE";

    // Polls
    public const string PollView = "POLL.VIEW";
    public const string PollVote = "POLL.VOTE";
    public const string PollCreate = "POLL.CREATE";

    // Chat & Messaging
    public const string ChatAccess = "CHAT.ACCESS";
    public const string ChatSend = "CHAT.SEND";

    // Administration & RBAC
    public const string AdminUserView = "ADMIN.USER.VIEW";
    public const string AdminUserManage = "ADMIN.USER.MANAGE";
    public const string AdminRbacManage = "ADMIN.RBAC.MANAGE";

    public static readonly IReadOnlyList<PermissionDefinition> All =
    [
        new(CommunityView, "View Communities", "Community", "/communities", true, true),
        new(CommunityCreate, "Create Community", "Community", "/communities/create", true, false),
        new(CommunityManage, "Manage Community Settings", "Community", null, true, false),
        new(PostView, "View Feed Posts", "Post", "/feed", true, true),
        new(PostCreate, "Create Post", "Post", "/feed", true, true),
        new(PostDelete, "Delete Post / Moderation", "Post", null, true, false),
        new(PollView, "View Polls", "Poll", "/polls", true, true),
        new(PollVote, "Vote on Polls", "Poll", "/polls", true, true),
        new(PollCreate, "Create Polls", "Poll", "/polls/create", true, false),
        new(ChatAccess, "Access Chat Rooms", "Chat", "/chat", true, true),
        new(ChatSend, "Send Chat Messages", "Chat", "/chat", true, true),
        new(AdminUserView, "View Admin Users", "Admin", "/admin/users", true, false),
        new(AdminUserManage, "Manage Admin Users", "Admin", "/admin/users", true, false),
        new(AdminRbacManage, "Manage RBAC Matrix", "Admin", "/admin/rbac", true, false)
    ];
}
```

### 1.5. Feature Models & DTOs
* **`Features/Authentication/AuthenticationModels.cs`**: `LoginRequestModel`, `LoginResponseModel`, `ChangePasswordRequestModel`, `RegisterRequestModel`.
* **`Features/Community/CommunityModels.cs`**: `CommunitySummaryModel`, `CreateCommunityRequestModel`, `JoinRequestModel`.
* **`Features/Post/PostModels.cs`**: `PostDetailModel`, `CreatePostRequestModel`, `CommentModel`, `LikeModel`.
* **`Features/Poll/PollModels.cs`**: `PollDetailModel`, `VoteRequestModel`, `CreatePollRequestModel`.
* **`Features/Chat/ChatModels.cs`**: `ConversationModel`, `ChatMessageModel`, `SendMessageRequestModel`.

---

## 3. Phase 2: Domain Logic & Security Services (`CommunityLink.Domain`)

### 2.1. Feature Manager & DI Composition (`FeatureManager.cs`)
* **Path**: `CommunityLink.Domain/FeatureManager.cs`
* **Purpose**: Configures DbContext with SQL Server / InMemory fallback and registers all domain services.
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Domain.Features.Authentication;
using CommunityLink.Domain.Features.RoleAndPermission;

namespace CommunityLink.Domain;

public static class FeatureManager
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddScoped(_ =>
        {
            var options = new DbContextOptionsBuilder<AppDbContext>();
            if (!string.IsNullOrWhiteSpace(connectionString))
                options.UseSqlServer(connectionString);
            else
                options.UseInMemoryDatabase("CommunityLinkInMemoryDb");

            return options.Options;
        });
        services.AddScoped<AppDbContext>();

        // Security & RBAC
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
        services.AddSingleton<ITokenIssuer, JwtTokenIssuer>();
        services.AddScoped<IPermissionEvaluator, PermissionEvaluator>();

        // Feature Services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IRoleAndPermissionService, RoleAndPermissionService>();

        return services;
    }
}
```

### 2.2. Current User Context & Token Issuer
* **`Security/CurrentUserContext.cs`**: Extracts `UserId`, `UserName`, `RoleId`, `Email` from `ClaimsPrincipal`.
* **`Security/TokenIssuer.cs`**: Generates signed JWT tokens with claims (`sub`, `name`, `email`, `role`, `session_id`).
* **`Security/PermissionAuthorization.cs`**: Real-time DB lookup via `IPermissionEvaluator` on every authorized request.

### 2.3. Feature Service Implementations
* **`Features/Authentication/`**: `AuthenticationService.cs`, `AuthenticationController.cs`.
* **`Features/RoleAndPermission/`**: `RoleAndPermissionService.cs`, `RoleAndPermissionController.cs`, `RbacSeeder.cs` (seeds Admin, Moderator, Member roles and permissions).
* **`Features/Community/`**: `CommunityService.cs`, `CommunityController.cs`.
* **`Features/Post/`**: `PostService.cs`, `PostController.cs`.
* **`Features/Poll/`**: `PollService.cs`, `PollController.cs`.
* **`Features/Chat/`**: `ChatService.cs`, `ChatController.cs`.

---

## 4. Phase 3: API Host & Middlewares (`CommunityLink.Api`)

### 3.1. Pipeline Middlewares
* **`Middlewares/ExceptionHandlingMiddleware.cs`**: Catches unhandled exceptions and outputs JSON `Result<object>` with HTTP 500.
* **`Middlewares/ApiRequestLoggingMiddleware.cs`**: Logs incoming requests, routes, HTTP verbs, user ID, status codes, and latency (ms).
* **`Middlewares/PasswordChangeRequirementMiddleware.cs`**: Restricts accounts with `MustChangePassword == true` until credentials are reset.

### 3.2. Program Pipeline Setup (`Program.cs`)
```csharp
using CommunityLink.Domain;
using CommunityLink.Api.Middlewares;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Features.RoleAndPermission;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers & OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "CommunityLinkDefaultDevSecretSigningKey1234567890!";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "CommunityLink.Api";
var audience = builder.Configuration["Jwt:Audience"] ?? "CommunityLink.Clients";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

// Domain Services
builder.Services.AddDomainServices(builder.Configuration);

var app = builder.Build();

// Seed RBAC on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await RbacSeeder.SeedAsync(db);
}

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ApiRequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<PasswordChangeRequirementMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

---

## 5. Phase 4: Blazor Client Portal (`CommunityLink.App`)

### 4.1. Cookie Authentication Setup (`Program.cs`)
* Configures `CookieAuthenticationDefaults` with `Cookie.Name = "Ace.Community.Auth"`, `HttpOnly = true`, `SameSite = SameSiteMode.Lax`.
* Registers `IHttpClientFactory` named client `"CommunityApi"` with base URL and authorization token delegation.
* Registers typed client API services (`AuthenticationApiService`, `CommunityApiService`, `PostApiService`, etc.).

### 4.2. Tailwind CSS Pipeline Setup
1. Initialize npm in `CommunityLink.App`:
   ```powershell
   npm init -y
   npm install -D tailwindcss postcss autoprefixer
   npx tailwindcss init -p
   ```
2. Configure `tailwind.config.js`:
   ```javascript
   module.exports = {
     content: ["./Components/**/*.{razor,html}", "./Pages/**/*.{razor,html}"],
     darkMode: 'class',
     theme: { extend: {} },
     plugins: []
   };
   ```
3. Create `Styles/app.css` with `@tailwind base; @tailwind components; @tailwind utilities;` and compile to `wwwroot/css/app.css`.

### 4.3. UI Layouts & Core Pages
* **`Components/Layout/MainLayout.razor`**: Responsive sidebar navigation, user badge, theme toggle, and SignalR reconnect modal.
* **`Components/Layout/NavMenu.razor`**: Role-permission filtered navigation menu.
* **`Services/ToastService.cs`**: Push notifications for UI action feedback.
* **Razor Pages**:
  * `/login` & `/register` (`Login.razor`, `Register.razor`)
  * `/communities` & `/communities/create` (`Communities.razor`, `CreateCommunity.razor`)
  * `/feed` (`Feed.razor` with post composer, likes, comments)
  * `/polls` (`Polls.razor` with real-time voting and percentage bar graphs)
  * `/chat` (`Chat.razor` with live conversation thread)
  * `/admin/rbac` (`RolePermissions.razor` for dynamic permission matrix toggling)

---

## 6. Phase 5: Automated Testing (`CommunityLink.Api.Tests` & `CommunityLink.App.Tests`)

1. **`CommunityLink.Api.Tests`**:
   * `Infrastructure/CommunityApiFactory.cs`: WebApplicationFactory with in-memory DB and test auth handlers.
   * `Security/DynamicRbacTests.cs`: Verifies endpoints return 401 Unauthorized for anonymous and 403 Forbidden when permissions are revoked.
   * `Features/CommunityEndpointTests.cs`: Validates CRUD and business workflows.
2. **`CommunityLink.App.Tests`**:
   * `Navigation/NavMenuRenderTests.cs`: Verifies menu items render only when matching permission claims exist.
   * `Pages/RouteAuthorizationTests.cs`: Ensures unauthorized routes redirect to `/login` or `/access-denied`.

---

## 7. Execution Checklist

- [ ] **Step 1**: Create Shared primitives (`PagedResult.cs`, `BaseController.cs`, `PermissionCatalog.cs`, DTOs).
- [ ] **Step 2**: Implement Domain DI (`FeatureManager.cs`), Security services, and RBAC Seeder.
- [ ] **Step 3**: Configure API middlewares, JWT Authentication, and Swagger in `CommunityLink.Api`.
- [ ] **Step 4**: Set up Tailwind CSS and Cookie Auth in `CommunityLink.App`.
- [ ] **Step 5**: Build out the Blazor UI pages and API service clients.
- [ ] **Step 6**: Add API and UI automated tests to ensure 100% build and test coverage.