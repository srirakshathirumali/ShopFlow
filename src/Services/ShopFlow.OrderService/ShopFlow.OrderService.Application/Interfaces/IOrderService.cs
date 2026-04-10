using ShopFlow.OrderService.Application.DTOs;

namespace ShopFlow.OrderService.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto request);
    Task<OrderResponseDto> GetOrderByIdAsync(Guid orderId);
    Task<IEnumerable<OrderResponseDto>> GetOrdersByCustomerAsync(Guid customerId);
    Task UpdateOrderStatusAsync(Guid orderId, Domain.Enums.OrderStatus status, string? reason = null);
}