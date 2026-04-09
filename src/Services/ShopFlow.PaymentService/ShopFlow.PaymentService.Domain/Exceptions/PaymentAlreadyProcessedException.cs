using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.PaymentService.Domain.Exceptions
{
    public class PaymentAlreadyProcessedException : Exception
    {
        public PaymentAlreadyProcessedException(Guid orderId)
            : base($"Payment for order '{orderId}' has already been processed.") { }
    }
}
