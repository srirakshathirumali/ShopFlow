using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.Contracts.Events
{
    public record OrderPlaced
    {
        public Guid OrderId { get; init; }
        public Guid CustomerId { get; init; }
        public List<OrderItem> Items { get; init; } = new();
        public decimal TotalAmount { get; init; }
        public DateTime PlacedAt { get; init; }
    }

    public record OrderItem
    {
        public Guid ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
    }
}
