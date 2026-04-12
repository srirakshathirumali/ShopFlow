using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopFlow.PaymentService.Domain.Interfaces;
using ShopFlow.PaymentService.Infrastructure.Consumers;
using ShopFlow.PaymentService.Infrastructure.Persistence;
using ShopFlow.PaymentService.Infrastructure.Persistence.Repositories;

namespace ShopFlow.PaymentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddDbContext<PaymentDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(
                    "ShopFlow.PaymentService.Infrastructure")));

        services.AddScoped<IPaymentRepository, PaymentRepository>();

        // MassTransit with consumers
        services.AddMassTransit(x =>
        {
            x.AddConsumer<InventoryReservedConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"]!);
                    h.Password(configuration["RabbitMQ:Password"]!);
                });

                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}