using System.Net;
using Tawzeef.Domain.Exceptions;

namespace Tawzeef.Api.Middleware;

/// <summary>BACKEND_GUIDELINES.md §13. Registered before auth, so auth failures are shaped too.</summary>
public class GlobalErrorHandlingMiddleware(RequestDelegate next, ILogger<GlobalErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteProblem(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (BadRequestException ex)
        {
            await WriteProblem(context, HttpStatusCode.BadRequest, ex.Message, ex.Errors);
        }
        catch (ConflictException ex)
        {
            await WriteProblem(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteProblem(context, HttpStatusCode.Unauthorized, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception on {Path}", context.Request.Path);
            await WriteProblem(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblem(HttpContext context, HttpStatusCode status, string title, List<string>? errors = null)
    {
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/problem+json";

        var problem = new { title, status = (int)status, errors };
        await context.Response.WriteAsJsonAsync(problem);
    }
}

public static class GlobalErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<GlobalErrorHandlingMiddleware>();
}
