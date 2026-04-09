using ShopFlow.PaymentService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.PaymentService.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public string? FailureReason { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
