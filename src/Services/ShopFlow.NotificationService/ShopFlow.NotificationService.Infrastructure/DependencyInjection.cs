using MassTransit;
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

        // MassTransit with consumers
        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderPlacedConsumer>();
            x.AddConsumer<InventoryReservedConsumer>();
            x.AddConsumer<PaymentProcessedConsumer>();
            x.AddConsumer<PaymentFailedConsumer>();
            x.AddConsumer<OrderCancelledConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"]!);
                    h.Password(configuration["RabbitMQ:Password"]!);
                    // Retry connection if RabbitMQ not ready
                    h.RequestedConnectionTimeout(TimeSpan.FromSeconds(10));
                });

                cfg.ReceiveEndpoint("shopflow-notification-order-placed", e =>
                {
                    e.ConfigureConsumer<OrderPlacedConsumer>(ctx);
                });

                cfg.ReceiveEndpoint("shopflow-notification-inventory-reserved", e =>
                {
                    e.ConfigureConsumer<InventoryReservedConsumer>(ctx);
                });

                cfg.ReceiveEndpoint("shopflow-notification-payment-processed", e =>
                {
                    e.ConfigureConsumer<PaymentProcessedConsumer>(ctx);
                });

                cfg.ReceiveEndpoint("shopflow-notification-payment-failed", e =>
                {
                    e.ConfigureConsumer<PaymentFailedConsumer>(ctx);
                });

                cfg.ReceiveEndpoint("shopflow-notification-order-cancelled", e =>
                {
                    e.ConfigureConsumer<OrderCancelledConsumer>(ctx);
                });
            });
        });

        return services;
    }
}