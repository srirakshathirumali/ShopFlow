using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.InventoryService.Domain.Exceptions
{
    public class InsufficientStockException : Exception
    {
        public InsufficientStockException(Guid productId, int requested, int available)
            : base($"Insufficient stock for product '{productId}'. Requested: {requested}, Available: {available}.") { }
    }
}
