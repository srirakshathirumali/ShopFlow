using MassTransit;
using ShopFlow.Contracts.Events;
using ShopFlow.NotificationService.Application.Interfaces;

public class OrderPlacedConsumer : IConsumer<OrderPlaced>
{
    private readonly INotificationEventHandler _eventHandler;
    public OrderPlacedConsumer(INotificationEventHandler eventHandler)
        => _eventHandler = eventHandler;
    public async Task Consume(ConsumeContext<OrderPlaced> context)
        => await _eventHandler.HandleOrderPlacedAsync(context.Message);
}