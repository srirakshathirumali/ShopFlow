using Microsoft.Extensions.DependencyInjection;
using ShopFlow.NotificationService.Application.Interfaces;

namespace ShopFlow.NotificationService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<INotificationService,Services.NotificationService>();

        return services;
    }
}