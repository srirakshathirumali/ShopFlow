using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopFlow.OrderService.Application.Interfaces;
using ShopFlow.OrderService.Domain.Interfaces;
using ShopFlow.OrderService.Infrastructure.BackgroundServices;
using ShopFlow.OrderService.Infrastructure.Consumers;
using ShopFlow.OrderService.Infrastructure.Messaging;
using ShopFlow.OrderService.Infrastructure.Persistence;
using ShopFlow.OrderService.Infrastructure.Persistence.Repositories;

namespace ShopFlow.OrderService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddDbContext<OrderDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(
                    "ShopFlow.OrderService.Infrastructure")));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        // Event publisher
        services.AddScoped<IEventPublisher, EventPublisher>();

        // Outbox background processor
        services.AddHostedService<OutboxProcessor>();

        // MassTransit — publisher only
        services.AddMassTransit(x =>
        {
            x.AddConsumer<InventoryReservedConsumer>();
            x.AddConsumer<InventoryReservationFailedConsumer>();
            x.AddConsumer<PaymentProcessedConsumer>();
            x.AddConsumer<PaymentFailedConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"]!);
                    h.Password(configuration["RabbitMQ:Password"]!);
                    // Retry connection if RabbitMQ not ready
                    h.RequestedConnectionTimeout(TimeSpan.FromSeconds(10));
                });

                cfg.ReceiveEndpoint("shopflow-order-inventory-reserved", e =>
                {
                    e.ConfigureConsumer<InventoryReservedConsumer>(ctx);
                });

                cfg.ReceiveEndpoint("shopflow-order-inventory-failed", e =>
                {
                    e.ConfigureConsumer<InventoryReservationFailedConsumer>(ctx);
                });

                cfg.ReceiveEndpoint("shopflow-order-payment-processed", e =>
                {
                    e.ConfigureConsumer<PaymentProcessedConsumer>(ctx);
                });

                cfg.ReceiveEndpoint("shopflow-order-payment-failed", e =>
                {
                    e.ConfigureConsumer<PaymentFailedConsumer>(ctx);
                });
            });
        });

        return services;
    }
}