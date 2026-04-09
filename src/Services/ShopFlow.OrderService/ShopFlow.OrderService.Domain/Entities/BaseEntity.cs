using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.OrderService.Domain.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
