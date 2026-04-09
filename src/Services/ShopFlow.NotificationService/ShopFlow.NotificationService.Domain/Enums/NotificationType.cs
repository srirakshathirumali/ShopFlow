using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.NotificationService.Domain.Enums
{
    public enum NotificationType
    {
        OrderPlaced = 1,
        InventoryReserved = 2,
        PaymentProcessed = 3,
        OrderConfirmed = 4,
        OrderCancelled = 5,
        PaymentFailed = 6
    }
}
