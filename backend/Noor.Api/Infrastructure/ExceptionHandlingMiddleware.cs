using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Noor.Core.Common;

namespace Noor.Api.Infrastructure;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            await WriteProblemAsync(context, ex.StatusCode, TitleFor(ex.StatusCode), ex.Message);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Database update conflict on {Path}", context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "That change could not be saved.",
                "The record may be in use or already exists. Refresh and try again.");
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected; no response needed.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Path}", context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Something went wrong.",
                "An unexpected error occurred. Please try again.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int status, string title, string detail)
    {
        if (context.Response.HasStarted) return;
        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Extensions = { ["traceId"] = context.TraceIdentifier }
        };

        await context.Response.WriteAsJsonAsync(problem);
    }

    private static string TitleFor(int status) => status switch
    {
        400 => "Please check your details.",
        401 => "Please sign in.",
        403 => "You don't have access to that.",
        404 => "Not found.",
        409 => "That action conflicts with existing data.",
        _ => "Request failed."
    };
}
