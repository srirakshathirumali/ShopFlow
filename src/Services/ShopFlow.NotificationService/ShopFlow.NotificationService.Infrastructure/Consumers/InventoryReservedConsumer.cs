using MassTransit;
using ShopFlow.Contracts.Events;
using ShopFlow.NotificationService.Application.Interfaces;

public class InventoryReservedConsumer : IConsumer<InventoryReserved>
{
    private readonly INotificationEventHandler _eventHandler;
    public InventoryReservedConsumer(INotificationEventHandler eventHandler)
        => _eventHandler = eventHandler;
    public async Task Consume(ConsumeContext<InventoryReserved> context)
        => await _eventHandler.HandleInventoryReservedAsync(context.Message);
}