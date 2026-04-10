using ShopFlow.OrderService.Domain.Enums;

namespace ShopFlow.OrderService.Application.DTOs;

public class OrderResponseDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderLineDto> OrderLines { get; set; } = new();
}
