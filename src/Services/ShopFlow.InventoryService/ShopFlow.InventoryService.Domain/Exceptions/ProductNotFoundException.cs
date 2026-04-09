using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.InventoryService.Domain.Exceptions
{
    public class ProductNotFoundException : Exception
    {
        public ProductNotFoundException(Guid productId)
            : base($"Product '{productId}' was not found.") { }
    }
}
