using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ShopFlow.NotificationService.Infrastructure.Persistence;

public class NotificationDbContextFactory
    : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(
                Directory.GetCurrentDirectory(),
                "../ShopFlow.NotificationService.API"))
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder =
            new DbContextOptionsBuilder<NotificationDbContext>();
        optionsBuilder.UseSqlServer(
            configuration.GetConnectionString("DefaultConnection"));

        return new NotificationDbContext(optionsBuilder.Options);
    }
}