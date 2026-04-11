using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopFlow.InventoryService.Domain.Interfaces;
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
        services.AddScoped<IStockReservationRepository,
            StockReservationRepository>();

        return services;
    }
}