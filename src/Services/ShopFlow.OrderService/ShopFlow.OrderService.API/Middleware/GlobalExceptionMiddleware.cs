using System.Net;
using System.Text.Json;
using ShopFlow.OrderService.Domain.Exceptions;

namespace ShopFlow.OrderService.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
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

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
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
            OrderNotFoundException e =>
                (HttpStatusCode.NotFound, "NOT_FOUND", e.Message),

            InvalidOrderStatusException e =>
                (HttpStatusCode.BadRequest, "INVALID_STATUS", e.Message),

            _ => (HttpStatusCode.InternalServerError,
                  "INTERNAL_ERROR",
                  "An unexpected error occurred.")
        };
}