using MassTransit;
using ShopFlow.Contracts.Events;
using ShopFlow.InventoryService.Application.Interfaces;

namespace ShopFlow.InventoryService.Infrastructure.Consumers;

public class OrderPlacedConsumer : IConsumer<OrderPlaced>
{
    private readonly IInventoryEventHandler _eventHandler;

    public OrderPlacedConsumer(IInventoryEventHandler eventHandler)
    {
        _eventHandler = eventHandler;
    }

    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        await _eventHandler.HandleOrderPlacedAsync(context.Message);
    }
}