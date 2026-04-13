using MassTransit;
using ShopFlow.Contracts.Events;
using ShopFlow.OrderService.Application.Interfaces;

namespace ShopFlow.OrderService.Infrastructure.Consumers;

public class InventoryReservationFailedConsumer:IConsumer<InventoryReservationFailed>
{
    private readonly IOrderEventHandler _eventHandler;

    public InventoryReservationFailedConsumer(IOrderEventHandler eventHandler)
    {
        _eventHandler = eventHandler;
    }

    public async Task Consume(ConsumeContext<InventoryReservationFailed> context)
    {
        await _eventHandler.HandleInventoryReservationFailedAsync(context.Message);
    }
}