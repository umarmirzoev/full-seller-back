using System.Net;
using System.Text.Json;
using FullSeller.Domain.Exceptions;

namespace FullSeller.WebApi.Middleware;

/// <summary>Единая точка перехвата исключений: превращает доменные и системные ошибки
/// в предсказуемый JSON-ответ { message } с корректным HTTP-статусом, вместо утечки stack trace клиенту.</summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            var (statusCode, message) = ex switch
            {
                NotFoundException => (HttpStatusCode.NotFound, ex.Message),
                ValidationException => (HttpStatusCode.BadRequest, ex.Message),
                UnauthorizedDomainException => (HttpStatusCode.Forbidden, ex.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ex.Message),
                InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
                _ => (HttpStatusCode.InternalServerError, "Внутренняя ошибка сервера. Попробуйте позже."),
            };

            if (statusCode == HttpStatusCode.InternalServerError)
                _logger.LogError(ex, "Необработанное исключение при обработке {Method} {Path}", context.Request.Method, context.Request.Path);
            else
                _logger.LogWarning("{ExceptionType}: {Message}", ex.GetType().Name, ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { message }));
        }
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseFullSellerExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionMiddleware>();
}
