using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ShopFlow.NotificationService.Application;
using ShopFlow.NotificationService.Infrastructure;
using ShopFlow.NotificationService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var retries = 0;
    const int maxRetries = 10;

    while (retries < maxRetries)
    {
        try
        {
            var db = scope.ServiceProvider
                .GetRequiredService<NotificationDbContext>();
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

            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    }
}
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "ShopFlow NotificationService";
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
app.Run();