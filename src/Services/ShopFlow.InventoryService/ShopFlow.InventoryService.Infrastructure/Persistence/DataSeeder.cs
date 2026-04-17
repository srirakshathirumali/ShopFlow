using Microsoft.EntityFrameworkCore;
using ShopFlow.InventoryService.Domain.Entities;

namespace ShopFlow.InventoryService.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(InventoryDbContext context)
    {
        if (await context.Products.AnyAsync()) return;

        var products = new List<Product>
        {
            new()
            {
                Id            = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa7"),
                Name          = "Laptop",
                SKU           = "LAP-001",
                StockQuantity = 100,
                Price         = 999.99m
            },
            new()
            {
                Id            = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa8"),
                Name          = "Mouse",
                SKU           = "MOU-001",
                StockQuantity = 200,
                Price         = 29.99m
            },
            new()
            {
                Id            = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa9"),
                Name          = "Keyboard",
                SKU           = "KEY-001",
                StockQuantity = 150,
                Price         = 79.99m
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
}