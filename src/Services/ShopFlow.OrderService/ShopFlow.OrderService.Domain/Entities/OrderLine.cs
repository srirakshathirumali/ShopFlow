using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.OrderService.Domain.Entities
{
    public class OrderLine : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;

        public Order Order { get; set; } = null!;
    }
}
