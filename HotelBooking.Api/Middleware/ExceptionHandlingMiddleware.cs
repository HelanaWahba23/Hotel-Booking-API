using HotelBooking.Api.Common;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ApiException exception)
        {
            context.Response.StatusCode = exception.StatusCode;
            await context.Response.WriteAsJsonAsync(new { error = exception.Message });
        }
        catch (DbUpdateException exception)
        {
            logger.LogWarning(exception, "A database constraint rejected the request.");
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(new { error = "The request conflicts with existing data." });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled API error.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
        }
    }
}
