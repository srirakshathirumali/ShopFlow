using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.OrderService.Domain.Exceptions
{
    public class OrderNotFoundException : Exception
    {
        public OrderNotFoundException(Guid orderId)
            : base($"Order '{orderId}' was not found.") { }
    }
}
