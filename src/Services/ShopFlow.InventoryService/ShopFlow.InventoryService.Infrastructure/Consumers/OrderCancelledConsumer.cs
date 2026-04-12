using MassTransit;
using ShopFlow.Contracts.Events;
using ShopFlow.InventoryService.Application.Interfaces;

namespace ShopFlow.InventoryService.Infrastructure.Consumers;

public class OrderCancelledConsumer : IConsumer<OrderCancelled>
{
    private readonly IInventoryEventHandler _eventHandler;

    public OrderCancelledConsumer(IInventoryEventHandler eventHandler)
    {
        _eventHandler = eventHandler;
    }

    public async Task Consume(ConsumeContext<OrderCancelled> context)
    {
        await _eventHandler.HandleOrderCancelledAsync(context.Message);
    }
}