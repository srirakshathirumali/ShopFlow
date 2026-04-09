using ShopFlow.InventoryService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.InventoryService.Domain.Entities
{
    public class StockReservation : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public ReservationStatus Status { get; set; }
        public DateTime? ReleasedAt { get; set; }

        public Product Product { get; set; } = null!;
    }
}
