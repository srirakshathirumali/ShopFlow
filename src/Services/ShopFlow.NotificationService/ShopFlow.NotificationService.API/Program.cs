using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ShopFlow.Contracts.HealthChecks;
using ShopFlow.NotificationService.Application;
using ShopFlow.NotificationService.Infrastructure;
using ShopFlow.NotificationService.Infrastructure.Hubs;
using ShopFlow.NotificationService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddApplication();
builder.Services.AddHealthChecks()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "notificationdb",
        tags: new[] { "db", "sql" });
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
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

            await Task.Delay(TimeSpan.FromSeconds(5));
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
if (!app.Environment.IsEnvironment("Docker"))
    app.UseHttpsRedirection();

app.UseCors();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();
// Map the hub — same line, different namespace
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = (ctx, report) =>
        HealthCheckResponseWriter.WriteResponse(ctx, report, "NotificationService")
});

app.Run();