using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ShopFlow.InventoryService.API.Middleware;
using ShopFlow.InventoryService.Application;
using ShopFlow.InventoryService.Infrastructure;
using ShopFlow.InventoryService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddHealthChecks()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "inventorydb",
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
                .GetRequiredService<InventoryDbContext>();
            db.Database.Migrate();
            await DataSeeder.SeedAsync(db);
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
        options.Title = "ShopFlow InventoryService";  
    });
}

if (app.Environment.IsEnvironment("Docker"))
{
    // Give RabbitMQ extra time to be fully ready
    await Task.Delay(TimeSpan.FromSeconds(5));
}
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            service = "InventoryService",
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds
            })
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});

app.Run();