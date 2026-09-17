using System.Text.Json;
using CommunityLink.Shared;

namespace CommunityLink.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled server error occurred while processing route: {Path}", context.Request.Path);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var result = Result.Failure(
                ex is InvalidOperationException or ArgumentException ? ex.Message : "An unexpected server error occurred.",
                ResultStatus.SystemError);

            await context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }
    }
}