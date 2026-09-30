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
        catch (Exception ex) when (context.RequestAborted.IsCancellationRequested ||
                                   ex is OperationCanceledException ||
                                   (ex is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Message.Contains("Operation cancelled by user", StringComparison.OrdinalIgnoreCase)))
        {
            logger.LogDebug("Request {Path} was canceled before completion.", context.Request.Path);
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