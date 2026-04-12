using ShopFlow.Contracts.Events;

namespace ShopFlow.PaymentService.Application.Interfaces;

public interface IPaymentEventHandler
{
    Task HandleInventoryReservedAsync(InventoryReserved inventoryReserved);
}