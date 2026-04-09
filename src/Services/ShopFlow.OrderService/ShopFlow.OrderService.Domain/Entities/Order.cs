using ShopFlow.OrderService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.OrderService.Domain.Entities
{
    public class Order : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string? CancellationReason { get; set; }
        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
    }
}
