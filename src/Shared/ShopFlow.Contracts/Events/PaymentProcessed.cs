namespace ShopFlow.Contracts.Events;

public record PaymentProcessed
{
    public Guid OrderId { get; init; }
    public Guid PaymentId { get; init; }
    public decimal Amount { get; init; }
    public DateTime ProcessedAt { get; init; }
}