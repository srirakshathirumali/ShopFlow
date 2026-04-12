using ShopFlow.Contracts.Events;

namespace ShopFlow.InventoryService.Application.Interfaces;

public interface IInventoryEventHandler
{
    Task HandleOrderPlacedAsync(OrderPlaced orderPlaced);
    Task HandleOrderCancelledAsync(OrderCancelled orderCancelled);
    Task HandlePaymentFailedAsync(PaymentFailed paymentFailed);
}