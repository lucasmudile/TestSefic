using Sefic.Api.Application.Exceptions;

namespace Sefic.Api.Middleware;

public sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (PartnerException ex)
        {
            await WriteErrorAsync(context, ex.Status, ex.ErrorCode, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "UNEXPECTED_ERROR",
                "Ocorreu um erro inesperado. Tente novamente.");
        }
    }

    private static Task WriteErrorAsync(HttpContext context, int status, string errorCode, string message)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsJsonAsync(new ApiErrorResponse(status, errorCode, message));
    }
}
