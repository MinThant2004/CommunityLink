using System.Diagnostics;

namespace CommunityLink.Api.Middlewares;

public sealed class ApiRequestLoggingMiddleware(RequestDelegate next, ILogger<ApiRequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var path = context.Request.Path.Value;
        var method = context.Request.Method;

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsed = stopwatch.ElapsedMilliseconds;

            if (statusCode >= 500)
            {
                logger.LogError("HTTP {Method} {Path} responded {StatusCode} in {Elapsed}ms", method, path, statusCode, elapsed);
            }
            else if (statusCode >= 400)
            {
                logger.LogWarning("HTTP {Method} {Path} responded {StatusCode} in {Elapsed}ms", method, path, statusCode, elapsed);
            }
            else
            {
                logger.LogInformation("HTTP {Method} {Path} responded {StatusCode} in {Elapsed}ms", method, path, statusCode, elapsed);
            }
        }
    }
}