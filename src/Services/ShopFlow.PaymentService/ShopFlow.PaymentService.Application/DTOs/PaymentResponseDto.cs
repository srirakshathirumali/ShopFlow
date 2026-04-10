using ShopFlow.PaymentService.Domain.Enums;

namespace ShopFlow.PaymentService.Application.DTOs;

public class PaymentResponseDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}