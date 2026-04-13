using ShopFlow.Contracts.Events;

namespace ShopFlow.OrderService.Application.Interfaces;

public interface IOrderEventHandler
{
    Task HandleInventoryReservedAsync(InventoryReserved inventoryReserved);
    Task HandleInventoryReservationFailedAsync(InventoryReservationFailed inventoryReservationFailed);
    Task HandlePaymentProcessedAsync(PaymentProcessed paymentProcessed);
    Task HandlePaymentFailedAsync(PaymentFailed paymentFailed);
}