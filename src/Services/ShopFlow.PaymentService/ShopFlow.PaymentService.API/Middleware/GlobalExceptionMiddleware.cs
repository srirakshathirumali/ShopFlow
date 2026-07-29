using ShopFlow.PaymentService.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace ShopFlow.PaymentService.API.Middleware
{
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
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception on {Method} {Path}: {Message}",
                context.Request.Method,
                context.Request.Path,
                exception.Message);

            var (statusCode, errorCode, message) = MapException(exception);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                status = (int)statusCode,
                errorCode,
                message,
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));
        }

        private static (HttpStatusCode, string, string) MapException(Exception ex) => ex switch
        {
            PaymentNotFoundException e =>
                 (HttpStatusCode.NotFound, "NOT_FOUND", e.Message),

            PaymentAlreadyProcessedException e =>
                (HttpStatusCode.Conflict, "ALREADY_PROCESSED", e.Message),

            _ => (HttpStatusCode.InternalServerError,
                  "INTERNAL_ERROR",
                  "An unexpected error occurred.")
        };
    }

}
