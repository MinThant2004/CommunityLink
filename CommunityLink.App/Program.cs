using CommunityLink.App.Components;
using CommunityLink.App.Apis;
using CommunityLink.App.Services;
using CommunityLink.Shared.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Antiforgery;

var builder = WebApplication.CreateBuilder(args);

// Add Razor components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Ace.Community.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// Dynamic RBAC Policies
builder.Services.AddAuthorization(options =>
{
    foreach (var permission in PermissionCatalog.All)
    {
        options.AddPolicy(PermissionCatalog.PolicyPrefix + permission.Code, policy =>
            policy.RequireAuthenticatedUser().RequireClaim("permission", permission.Code));
    }
});

var apiBaseUrl = builder.Configuration["CommunityApi:BaseUrl"] ?? "http://localhost:5000";
var timeoutSec = builder.Configuration.GetValue<int>("CommunityApi:TimeoutSeconds", 30);

builder.Services.AddHttpClient("CommunityApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(timeoutSec);
});

// Register App Services & API Clients
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<AuthSessionService>();
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<AuthenticationApiService>();
builder.Services.AddScoped<CommunityApiService>();
builder.Services.AddScoped<PostApiService>();
builder.Services.AddScoped<PollApiService>();
builder.Services.AddScoped<ChatApiService>();
builder.Services.AddScoped<RoleAndPermissionApiService>();
builder.Services.AddScoped<AdministrationApiService>();
builder.Services.AddScoped<UserDashboardApiService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Logout Endpoint
app.MapPost("/account/logout", async (HttpContext context, IAntiforgery antiforgery) =>
{
    await antiforgery.ValidateRequestAsync(context);
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
}).RequireAuthorization();

// Login / Register Flow Endpoints (issue the Ace.Community.Auth cookie ticket)
app.MapAuthFlowEndpoints();

app.Run();

#pragma warning disable ASP0027
public partial class Program;
#pragma warning restore ASP0027