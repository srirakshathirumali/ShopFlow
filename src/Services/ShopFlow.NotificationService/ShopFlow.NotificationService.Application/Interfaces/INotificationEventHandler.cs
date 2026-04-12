using ShopFlow.Contracts.Events;

namespace ShopFlow.NotificationService.Application.Interfaces;

public interface INotificationEventHandler
{
    Task HandleOrderPlacedAsync(OrderPlaced orderPlaced);
    Task HandleInventoryReservedAsync(InventoryReserved inventoryReserved);
    Task HandlePaymentProcessedAsync(PaymentProcessed paymentProcessed);
    Task HandlePaymentFailedAsync(PaymentFailed paymentFailed);
    Task HandleOrderCancelledAsync(OrderCancelled orderCancelled);
}