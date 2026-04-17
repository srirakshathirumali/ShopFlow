using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.InventoryService.Application.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : class;
    }
}
