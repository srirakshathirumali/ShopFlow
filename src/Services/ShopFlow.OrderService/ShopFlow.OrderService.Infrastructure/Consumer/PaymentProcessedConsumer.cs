using MassTransit;
using ShopFlow.Contracts.Events;
using ShopFlow.OrderService.Application.Interfaces;

namespace ShopFlow.OrderService.Infrastructure.Consumers;

public class PaymentProcessedConsumer : IConsumer<PaymentProcessed>
{
    private readonly IOrderEventHandler _eventHandler;

    public PaymentProcessedConsumer(IOrderEventHandler eventHandler)
    {
        _eventHandler = eventHandler;
    }

    public async Task Consume(ConsumeContext<PaymentProcessed> context)
    {
        await _eventHandler.HandlePaymentProcessedAsync(context.Message);
    }
}