using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ShopFlow.Contracts.HealthChecks;
using ShopFlow.OrderService.API.Middleware;
using ShopFlow.OrderService.Application;
using ShopFlow.OrderService.Infrastructure;
using ShopFlow.OrderService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "orderdb",
        tags: new[] { "db", "sql" });

var app = builder.Build();
// Auto-apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var retries = 0;
    const int maxRetries = 10;

    while (retries < maxRetries)
    {
        try
        {
            var db = scope.ServiceProvider
                .GetRequiredService<OrderDbContext>();
            db.Database.Migrate();
            break;
        }
        catch (Exception ex)
        {
            retries++;
            Console.WriteLine(
                $"Migration attempt {retries} failed: {ex.Message}. " +
                $"Retrying in 5 seconds...");

            if (retries >= maxRetries)
                throw;

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "ShopFlow OrderService";
    });
}
if (app.Environment.IsEnvironment("Docker"))
{
    // Give RabbitMQ extra time to be fully ready
    await Task.Delay(TimeSpan.FromSeconds(5));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = (ctx, report) =>
        HealthCheckResponseWriter.WriteResponse(ctx, report, "OrderService")
});

app.Run();