using Microsoft.Extensions.DependencyInjection;
using ShopFlow.NotificationService.Application.Interfaces;
using ShopFlow.NotificationService.Application.Services;

namespace ShopFlow.NotificationService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<INotificationService,Services.NotificationService>();
        services.AddScoped<INotificationEventHandler, NotificationEventHandler>();

        return services;
    }
}