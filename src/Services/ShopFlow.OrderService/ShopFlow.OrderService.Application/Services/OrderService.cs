using ShopFlow.OrderService.Application.DTOs;
using ShopFlow.OrderService.Application.Interfaces;
using ShopFlow.OrderService.Domain.Entities;
using ShopFlow.OrderService.Domain.Enums;
using ShopFlow.OrderService.Domain.Exceptions;
using ShopFlow.OrderService.Domain.Interfaces;

namespace ShopFlow.OrderService.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(
        CreateOrderRequestDto request)
    {
        var order = new Order
        {
            CustomerId = request.CustomerId,
            Status = OrderStatus.Pending,
            TotalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice),
            OrderLines = request.Items.Select(i => new OrderLine
            {
                Id = Guid.NewGuid(),
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        await _orderRepository.AddAsync(order);
        return MapToDto(order);
    }

    public async Task<OrderResponseDto> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new OrderNotFoundException(orderId);

        return MapToDto(order);
    }

    public async Task<IEnumerable<OrderResponseDto>> GetOrdersByCustomerAsync(
        Guid customerId)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
        return orders.Select(MapToDto);
    }

    public async Task UpdateOrderStatusAsync(
        Guid orderId,
        OrderStatus status,
        string? reason = null)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new OrderNotFoundException(orderId);

        order.Status = status;

        if (reason is not null)
            order.CancellationReason = reason;

        await _orderRepository.UpdateAsync(order);
    }

    private static OrderResponseDto MapToDto(Order order) =>
        new()
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            CancellationReason = order.CancellationReason,
            CreatedAt = order.CreatedAt,
            OrderLines = order.OrderLines.Select(l => new OrderLineDto
            {
                ProductId = l.ProductId,
                ProductName = l.ProductName,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                LineTotal = l.LineTotal
            }).ToList()
        };
}