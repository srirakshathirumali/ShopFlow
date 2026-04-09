using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.NotificationService.Domain.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
