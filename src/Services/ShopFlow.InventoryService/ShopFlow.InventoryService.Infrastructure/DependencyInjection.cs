using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopFlow.InventoryService.Application.Interfaces;
using ShopFlow.InventoryService.Domain.Interfaces;
using ShopFlow.InventoryService.Infrastructure.Consumers;
using ShopFlow.InventoryService.Infrastructure.Messaging;
using ShopFlow.InventoryService.Infrastructure.Persistence;
using ShopFlow.InventoryService.Infrastructure.Persistence.Repositories;

namespace ShopFlow.InventoryService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(
                    "ShopFlow.InventoryService.Infrastructure")));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockReservationRepository, StockReservationRepository>();
        services.AddScoped<IEventPublisher, EventPublisher>();

        // MassTransit with consumers
        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderPlacedConsumer>();
            x.AddConsumer<OrderCancelledConsumer>();
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

                // Explicit queue names — avoids collision with NotificationService
                cfg.ReceiveEndpoint("shopflow-inventory-order-placed", e =>
                {
                    e.ConfigureConsumer<OrderPlacedConsumer>(ctx);
                });

                cfg.ReceiveEndpoint("shopflow-inventory-order-cancelled", e =>
                {
                    e.ConfigureConsumer<OrderCancelledConsumer>(ctx);
                });

                cfg.ReceiveEndpoint("shopflow-inventory-payment-failed", e =>
                {
                    e.ConfigureConsumer<PaymentFailedConsumer>(ctx);
                });
            });
        });

        return services;
    }
}