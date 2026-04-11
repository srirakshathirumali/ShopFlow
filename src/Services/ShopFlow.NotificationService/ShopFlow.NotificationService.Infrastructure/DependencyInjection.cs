using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopFlow.NotificationService.Domain.Interfaces;
using ShopFlow.NotificationService.Infrastructure.Persistence;
using ShopFlow.NotificationService.Infrastructure.Persistence.Repositories;

namespace ShopFlow.NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(
                    "ShopFlow.NotificationService.Infrastructure")));

        services.AddScoped<INotificationRepository, NotificationRepository>();

        return services;
    }
}