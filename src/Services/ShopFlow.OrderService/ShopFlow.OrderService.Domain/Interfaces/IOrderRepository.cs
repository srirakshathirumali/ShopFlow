using ShopFlow.OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.OrderService.Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id);
        Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
    }
}
