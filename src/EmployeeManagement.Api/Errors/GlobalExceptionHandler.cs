using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Errors;

/// <summary>Turns every unhandled exception into an RFC 7807 problem response. Unexpected errors are
/// logged and returned as 500 without leaking internal details.</summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problems) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception exception, CancellationToken ct)
    {
        var (status, title, detail) = exception switch
        {
            AppException app => (app.StatusCode, app.Title, app.Message),
            DbUpdateException => (StatusCodes.Status409Conflict, "Database conflict", "The change conflicts with existing data (for example a duplicate value or a record still in use)."),
            BadHttpRequestException bad => (bad.StatusCode, "Bad request", bad.Message),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error", "An unexpected error occurred. Please try again later.")
        };

        if (status >= 500) logger.LogError(exception, "Unhandled exception for {Method} {Path}", http.Request.Method, http.Request.Path);
        else logger.LogInformation("Request failed with {Status}: {Detail}", status, detail);

        http.Response.StatusCode = status;
        return await problems.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = http,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = detail, Instance = http.Request.Path }
        });
    }
}
