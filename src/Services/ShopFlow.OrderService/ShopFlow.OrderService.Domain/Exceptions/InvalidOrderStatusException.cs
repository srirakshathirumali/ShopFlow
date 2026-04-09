using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.OrderService.Domain.Exceptions
{
    public class InvalidOrderStatusException : Exception
    {
        public InvalidOrderStatusException(string message)
            : base(message) { }
    }
}
