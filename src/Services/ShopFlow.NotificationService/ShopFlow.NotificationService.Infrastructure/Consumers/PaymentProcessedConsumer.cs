using MassTransit;
using ShopFlow.Contracts.Events;
using ShopFlow.NotificationService.Application.Interfaces;

public class PaymentProcessedConsumer : IConsumer<PaymentProcessed>
{
    private readonly INotificationEventHandler _eventHandler;
    public PaymentProcessedConsumer(INotificationEventHandler eventHandler)
        => _eventHandler = eventHandler;
    public async Task Consume(ConsumeContext<PaymentProcessed> context)
        => await _eventHandler.HandlePaymentProcessedAsync(context.Message);
}