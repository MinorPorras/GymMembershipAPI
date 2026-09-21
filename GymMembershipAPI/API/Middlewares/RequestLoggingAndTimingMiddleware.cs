using System.Diagnostics;

namespace GymMembershipAPI.API.Middlewares;

public class RequestLoggingAndTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingAndTimingMiddleware> _logger;

    public RequestLoggingAndTimingMiddleware(RequestDelegate next, ILogger<RequestLoggingAndTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;

        _logger.LogInformation("[INICIO] {Method} {Path}", method, path);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation("[FIN] {Method} {Path} - Status: {StatusCode} Time: {ElapsedMs}ms",
                method, path, statusCode, elapsedMs);
        }
    }
}