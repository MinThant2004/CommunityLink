using System.Security.Claims;
using System.Text.Json;
using CommunityLink.Shared;

namespace CommunityLink.Api.Middlewares;

public sealed class PasswordChangeRequirementMiddleware(RequestDelegate next)
{
    private static readonly HashSet<string> AllowedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/auth/change-password",
        "/api/auth/me",
        "/api/auth/logout"
    };

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var mustChange = user.FindFirst("must_change_password")?.Value;
            if (bool.TryParse(mustChange, out var requiresChange) && requiresChange)
            {
                var path = context.Request.Path.Value ?? string.Empty;
                if (!AllowedPaths.Contains(path))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    var result = Result.Failure("Password change required before accessing other resources.", ResultStatus.Forbidden);
                    await context.Response.WriteAsync(JsonSerializer.Serialize(result));
                    return;
                }
            }
        }

        await next(context);
    }
}