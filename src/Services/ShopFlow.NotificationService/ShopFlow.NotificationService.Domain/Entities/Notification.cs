using ShopFlow.NotificationService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.NotificationService.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
    }
}
