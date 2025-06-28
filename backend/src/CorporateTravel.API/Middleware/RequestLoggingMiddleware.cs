using Microsoft.AspNetCore.Http;
using Serilog;
using System.Diagnostics;

namespace CorporateTravel.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
        _logger = Log.ForContext<RequestLoggingMiddleware>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            _logger.Information(
                "HTTP {RequestMethod} {RequestPath} started from {RemoteIpAddress}",
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress);

            await _next(context);

            stopwatch.Stop();

            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBodyStream);

            var statusCode = context.Response.StatusCode;
            var elapsed = stopwatch.ElapsedMilliseconds;

            if (statusCode >= 400)
            {
                _logger.Warning(
                    "HTTP {RequestMethod} {RequestPath} completed with status {StatusCode} in {Elapsed}ms",
                    context.Request.Method,
                    context.Request.Path,
                    statusCode,
                    elapsed);
            }
            else
            {
                _logger.Information(
                    "HTTP {RequestMethod} {RequestPath} completed with status {StatusCode} in {Elapsed}ms",
                    context.Request.Method,
                    context.Request.Path,
                    statusCode,
                    elapsed);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.Error(
                ex,
                "HTTP {RequestMethod} {RequestPath} failed after {Elapsed}ms",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
} 