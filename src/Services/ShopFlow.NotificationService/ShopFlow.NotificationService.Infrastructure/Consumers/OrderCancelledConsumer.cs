using MassTransit;
using ShopFlow.Contracts.Events;
using ShopFlow.NotificationService.Application.Interfaces;

public class OrderCancelledConsumer : IConsumer<OrderCancelled>
{
    private readonly INotificationEventHandler _eventHandler;
    public OrderCancelledConsumer(INotificationEventHandler eventHandler)
        => _eventHandler = eventHandler;
    public async Task Consume(ConsumeContext<OrderCancelled> context)
        => await _eventHandler.HandleOrderCancelledAsync(context.Message);
}