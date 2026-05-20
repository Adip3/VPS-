using System.Net;
using System.Text.Json;

namespace VPS.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _log;
    private readonly IWebHostEnvironment _env;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> log,
        IWebHostEnvironment env)
    {
        _next = next;
        _log = log;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }

        // ✅ Business errors
        catch (InvalidOperationException ex)
        {
            _log.LogWarning(ex, "Business rule violation");

            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            await ctx.Response.WriteAsJsonAsync(new
            {
                error = ex.Message,
                traceId = ctx.TraceIdentifier
            });
        }

        // ✅ Unauthorized
        catch (UnauthorizedAccessException ex)
        {
            _log.LogWarning(ex, "Unauthorized");

            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            await ctx.Response.WriteAsJsonAsync(new
            {
                error = "Unauthorized",
                traceId = ctx.TraceIdentifier
            });
        }

        // ✅ Everything else (IMPORTANT)
        catch (Exception ex)
        {
            _log.LogError(ex, "Unhandled exception");

            ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // 👇 Show detailed error ONLY in Development
            if (_env.IsDevelopment())
            {
                await ctx.Response.WriteAsJsonAsync(new
                {
                    error = ex.Message,
                    detail = ex.StackTrace,
                    traceId = ctx.TraceIdentifier
                });
            }
            else
            {
                // 👇 Production-safe response
                await ctx.Response.WriteAsJsonAsync(new
                {
                    error = "Internal server error",
                    traceId = ctx.TraceIdentifier
                });
            }
        }
    }
}