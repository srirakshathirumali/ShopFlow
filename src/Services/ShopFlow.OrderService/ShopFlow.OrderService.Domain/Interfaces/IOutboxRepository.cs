using ShopFlow.OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.OrderService.Domain.Interfaces
{
    public interface IOutboxRepository
    {
        Task AddAsync(OutboxMessage message);
        Task<IEnumerable<OutboxMessage>> GetUnprocessedAsync();
        Task MarkAsProcessedAsync(Guid messageId);
    }
}
