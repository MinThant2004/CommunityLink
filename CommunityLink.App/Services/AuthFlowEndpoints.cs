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
                   HttpContext context,
                   IAntiforgery antiforgery,
                   AuthenticationApiService authApi,
                   AuthSessionService sessions) =>
        {
            await antiforgery.ValidateRequestAsync(context);
            return await HandleLoginAsync(authApi, sessions, "/login", email, password, rememberMe == true, returnUrl);
        });

        endpoints.MapPost("/account/register",
            async ([FromForm] string fullName,
                   [FromForm] string userName,
                   [FromForm] string email,
                   [FromForm] string password,
                   [FromForm] string? confirmPassword,
                   HttpContext context,
                   IAntiforgery antiforgery,
                   AuthenticationApiService authApi) =>
        {
            await antiforgery.ValidateRequestAsync(context);
            var result = await authApi.RegisterAsync(new RegisterRequestModel(
                fullName, userName, email, password, Bio: null, ConfirmPassword: confirmPassword));

            if (!result.IsSuccess || result.Data is null)
                return RedirectWithError("/register", result.Message);

            return Results.Redirect("/login");
        });

        endpoints.MapPost("/account/admin/login",
            async ([FromForm] string email,
                   [FromForm] string password,
                   [FromForm] bool? rememberMe,
                   HttpContext context,
                   IAntiforgery antiforgery,
                   AuthenticationApiService authApi,
                   AuthSessionService sessions) =>
        {
            await antiforgery.ValidateRequestAsync(context);
            return await HandleLoginAsync(authApi, sessions, "/admin/portal-entry/login", email, password, rememberMe == true, "/admin");
        });

        endpoints.MapPost("/account/admin/register",
            async ([FromForm] string fullName,
                   [FromForm] string email,
                   [FromForm] string password,
                   [FromForm] string? confirmPassword,
                   [FromForm] string adminInviteCode,
                   HttpContext context,
                   IAntiforgery antiforgery,
                   AuthenticationApiService authApi) =>
        {
            await antiforgery.ValidateRequestAsync(context);
            var result = await authApi.RegisterAdminAsync(new RegisterAdminRequestModel(
                fullName, email, password, adminInviteCode, ConfirmPassword: confirmPassword));

            if (!result.IsSuccess || result.Data is null)
                return RedirectWithError("/admin/portal-entry/register", result.Message);

            return Results.Redirect("/admin/portal-entry/login");
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
            return RedirectWithError(loginPath, result.Message);

        await sessions.SignInAsync(result.Data, rememberMe);
        var target = IsLocalUrl(returnUrl) ? returnUrl : "/postupload";
        return Results.Redirect(target);
    }

    private static bool IsLocalUrl(string? url) =>
        url != null && url.StartsWith("/") && !url.StartsWith("//") && !url.StartsWith("/\\");

    private static IResult RedirectWithError(string path, string message) =>
        Results.Redirect($"{path}?error={Uri.EscapeDataString(message)}");
}