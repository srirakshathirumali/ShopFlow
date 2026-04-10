using Microsoft.EntityFrameworkCore;
using ShopFlow.InventoryService.Domain.Entities;
using ShopFlow.InventoryService.Domain.Interfaces;

namespace ShopFlow.InventoryService.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly InventoryDbContext _context;

    public ProductRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id) =>
        await _context.Products.FindAsync(id);

    public async Task<IEnumerable<Product>> GetAllAsync() =>
        await _context.Products
            .OrderBy(p => p.Name)
            .ToListAsync();

    public async Task AddAsync(Product product)
    {
        product.Id = Guid.NewGuid();
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }
}