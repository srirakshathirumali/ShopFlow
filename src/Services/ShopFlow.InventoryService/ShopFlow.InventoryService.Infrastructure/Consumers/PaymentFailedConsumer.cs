using MassTransit;
using ShopFlow.Contracts.Events;
using ShopFlow.InventoryService.Application.Interfaces;

namespace ShopFlow.InventoryService.Infrastructure.Consumers;

public class PaymentFailedConsumer : IConsumer<PaymentFailed>
{
    private readonly IInventoryEventHandler _eventHandler;

    public PaymentFailedConsumer(IInventoryEventHandler eventHandler)
    {
        _eventHandler = eventHandler;
    }

    public async Task Consume(ConsumeContext<PaymentFailed> context)
    {
        await _eventHandler.HandlePaymentFailedAsync(context.Message);
    }
}