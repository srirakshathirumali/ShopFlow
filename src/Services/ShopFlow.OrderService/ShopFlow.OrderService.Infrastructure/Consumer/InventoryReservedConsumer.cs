using MassTransit;
using ShopFlow.Contracts.Events;
using ShopFlow.OrderService.Application.Interfaces;

namespace ShopFlow.OrderService.Infrastructure.Consumers;

public class InventoryReservedConsumer : IConsumer<InventoryReserved>
{
    private readonly IOrderEventHandler _eventHandler;

    public InventoryReservedConsumer(IOrderEventHandler eventHandler)
    {
        _eventHandler = eventHandler;
    }

    public async Task Consume(ConsumeContext<InventoryReserved> context)
    {
        await _eventHandler.HandleInventoryReservedAsync(context.Message);
    }
}