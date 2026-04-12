using MassTransit;
using ShopFlow.Contracts.Events;
using ShopFlow.NotificationService.Application.Interfaces;

public class PaymentFailedConsumer : IConsumer<PaymentFailed>
{
    private readonly INotificationEventHandler _eventHandler;
    public PaymentFailedConsumer(INotificationEventHandler eventHandler)
        => _eventHandler = eventHandler;
    public async Task Consume(ConsumeContext<PaymentFailed> context)
        => await _eventHandler.HandlePaymentFailedAsync(context.Message);
}