using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.PaymentService.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending = 1,
        Processed = 2,
        Failed = 3,
        Refunded = 4
    }
}
