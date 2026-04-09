using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.OrderService.Domain.Entities
{
    public class OutboxMessage : BaseEntity
    {
        public string EventType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public bool IsProcessed { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public int RetryCount { get; set; }
    }
}
