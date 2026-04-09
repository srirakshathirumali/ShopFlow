using ShopFlow.InventoryService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.InventoryService.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
    }
}
