using CommunityLink.App.Apis;
using CommunityLink.Shared.Features.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace CommunityLink.App.Services;

public static class AuthFlowEndpoints
{
    public static IEndpointRouteBuilder MapAuthFlowEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/account/login",
            async ([FromForm] string email,
                   [FromForm] string password,
                   [FromForm] bool? rememberMe,
                   [FromForm] string? returnUrl,
                   AuthenticationApiService authApi,
                   AuthSessionService sessions) =>
        {
            return await HandleLoginAsync(authApi, sessions, "/login", email, password, rememberMe == true, returnUrl);
        }).DisableAntiforgery();

        endpoints.MapPost("/account/register",
            async ([FromForm] string fullName,
                   [FromForm] string userName,
                   [FromForm] string email,
                   [FromForm] string password,
                   [FromForm] string? confirmPassword,
                   AuthenticationApiService authApi) =>
        {
            var result = await authApi.RegisterAsync(new RegisterRequestModel(
                fullName, userName, email, password, Bio: null, ConfirmPassword: confirmPassword));

            if (!result.IsSuccess || result.Data is null)
            {
                var qs = new Dictionary<string, string?>
                {
                    ["error"] = result.Message,
                    ["fullName"] = fullName,
                    ["userName"] = userName,
                    ["email"] = email
                };
                return RedirectWithQuery("/register", qs);
            }

            return Results.Redirect("/login");
        }).DisableAntiforgery();

        endpoints.MapPost("/account/admin/login",
            async ([FromForm] string email,
                   [FromForm] string password,
                   [FromForm] bool? rememberMe,
                   AuthenticationApiService authApi,
                   AuthSessionService sessions) =>
        {
            var result = await authApi.LoginAdminAsync(new LoginRequestModel(email, password, rememberMe == true, IsAdmin: true));
            if (!result.IsSuccess || result.Data is null)
            {
                var qs = new Dictionary<string, string?>
                {
                    ["error"] = result.Message,
                    ["email"] = email
                };
                return RedirectWithQuery("/admin/portal-entry/login", qs);
            }

            await sessions.SignInAsync(result.Data, rememberMe == true);
            return Results.Redirect("/admin/dashboard");
        }).DisableAntiforgery();

        endpoints.MapPost("/account/delete",
            async (HttpContext context,
                   IAntiforgery antiforgery,
                   UserProfileApiService profileApi,
                   AuthSessionService sessions) =>
        {
            await antiforgery.ValidateRequestAsync(context);
            var result = await profileApi.DeleteMyAccountAsync();
            await sessions.SignOutAsync();
            return Results.Redirect("/login?error=" + Uri.EscapeDataString("Your account has been closed successfully."));
        });

        return endpoints;
    }

    private static async Task<IResult> HandleLoginAsync(
        AuthenticationApiService authApi,
        AuthSessionService sessions,
        string loginPath,
        string email,
        string password,
        bool rememberMe,
        string? returnUrl)
    {
        var result = await authApi.LoginAsync(new LoginRequestModel(email, password, rememberMe));

        if (!result.IsSuccess || result.Data is null)
        {
            var qs = new Dictionary<string, string?>
            {
                ["error"] = result.Message,
                ["email"] = email
            };
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                qs["returnUrl"] = returnUrl;
            }
            return RedirectWithQuery(loginPath, qs);
        }

        await sessions.SignInAsync(result.Data, rememberMe);
        
        string defaultTarget = (result.Data.RoleCode?.ToUpperInvariant() == "ADMIN" || result.Data.RoleCode?.ToUpperInvariant() == "SUPERADMIN")
            ? "/admin/dashboard"
            : "/";

        var target = IsLocalUrl(returnUrl) ? returnUrl! : defaultTarget;
        return Results.Redirect(target);
    }

    private static bool IsLocalUrl(string? url) =>
        url != null && url.StartsWith("/") && !url.StartsWith("//") && !url.StartsWith("/\\");

    private static IResult RedirectWithQuery(string path, IDictionary<string, string?> queryParams)
    {
        var items = queryParams
            .Where(kv => !string.IsNullOrEmpty(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}");
        var queryString = string.Join("&", items);
        return Results.Redirect(string.IsNullOrEmpty(queryString) ? path : $"{path}?{queryString}");
    }
}