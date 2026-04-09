using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.OrderService.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 1,
        InventoryReserved = 2,
        PaymentProcessed = 3,
        Confirmed = 4,
        Cancelled = 5
    }
}
