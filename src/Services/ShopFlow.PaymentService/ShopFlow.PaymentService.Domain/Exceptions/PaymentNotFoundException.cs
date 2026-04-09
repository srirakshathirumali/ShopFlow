using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.PaymentService.Domain.Exceptions
{
    public class PaymentNotFoundException : Exception
    {
        public PaymentNotFoundException(Guid orderId)
            : base($"Payment for order '{orderId}' was not found.") { }
    }
}
