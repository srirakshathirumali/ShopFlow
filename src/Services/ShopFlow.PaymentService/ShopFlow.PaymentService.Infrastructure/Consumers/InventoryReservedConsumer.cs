using MassTransit;
using ShopFlow.Contracts.Events;
using ShopFlow.PaymentService.Application.Interfaces;

namespace ShopFlow.PaymentService.Infrastructure.Consumers;

public class InventoryReservedConsumer : IConsumer<InventoryReserved>
{
    private readonly IPaymentEventHandler _eventHandler;

    public InventoryReservedConsumer(IPaymentEventHandler eventHandler)
    {
        _eventHandler = eventHandler;
    }

    public async Task Consume(ConsumeContext<InventoryReserved> context)
    {
        await _eventHandler.HandleInventoryReservedAsync(context.Message);
    }
}