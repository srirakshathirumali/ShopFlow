using MassTransit;
using ShopFlow.Contracts.Events;
using ShopFlow.OrderService.Application.Interfaces;

namespace ShopFlow.OrderService.Infrastructure.Consumers;

public class PaymentFailedConsumer : IConsumer<PaymentFailed>
{
    private readonly IOrderEventHandler _eventHandler;

    public PaymentFailedConsumer(IOrderEventHandler eventHandler)
    {
        _eventHandler = eventHandler;
    }

    public async Task Consume(ConsumeContext<PaymentFailed> context)
    {
        await _eventHandler.HandlePaymentFailedAsync(context.Message);
    }
}