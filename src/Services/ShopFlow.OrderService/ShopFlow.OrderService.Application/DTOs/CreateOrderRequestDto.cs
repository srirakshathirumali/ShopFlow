namespace ShopFlow.OrderService.Application.DTOs;

public class CreateOrderRequestDto
{
    public Guid CustomerId { get; set; }
    public List<CreateOrderLineDto> Items { get; set; } = new();
}

