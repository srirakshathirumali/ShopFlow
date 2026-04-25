using ShopFlow.Contracts.Events;
using ShopFlow.OrderService.Application.DTOs;
using ShopFlow.OrderService.Application.Interfaces;
using ShopFlow.OrderService.Domain.Entities;
using ShopFlow.OrderService.Domain.Enums;
using ShopFlow.OrderService.Domain.Exceptions;
using ShopFlow.OrderService.Domain.Interfaces;
using System.Text.Json;

namespace ShopFlow.OrderService.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOutboxRepository _outboxRepository;
    
    public OrderService(IOrderRepository orderRepository, IOutboxRepository outboxRepository)
    {
        _orderRepository = orderRepository;
        _outboxRepository = outboxRepository;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequestDto request)
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

        // Build event before saving
        var orderPlacedEvent = new OrderPlaced
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = order.TotalAmount,
            PlacedAt = DateTime.UtcNow,
            Items = order.OrderLines.Select(l => new OrderItem
            {
                ProductId = l.ProductId,
                ProductName = l.ProductName,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            }).ToList()
        };

        // Save event to Outbox — atomically with the order
        await _outboxRepository.AddAsync(new OutboxMessage
        {
            EventType = typeof(OrderPlaced).FullName!,
            Payload = JsonSerializer.Serialize(orderPlacedEvent),
            IsProcessed = false,
            RetryCount = 0
        });

        return MapToDto(order);
    }

    public async Task<OrderResponseDto> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new OrderNotFoundException(orderId);

        return MapToDto(order);
    }

    public async Task<IEnumerable<OrderResponseDto>> GetOrdersByCustomerAsync(Guid customerId)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
        return orders.Select(MapToDto);
    }

    public async Task UpdateOrderStatusAsync(Guid orderId,OrderStatus status,string? reason = null)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new OrderNotFoundException(orderId);

        order.Status = status;

        if (reason is not null)
            order.CancellationReason = reason;

        await _orderRepository.UpdateAsync(order);
    }

    public async Task<OrderResponseDto> CancelOrderAsync(Guid orderId, string reason)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new OrderNotFoundException(orderId);

        if (order.Status == OrderStatus.Confirmed)
            throw new InvalidOrderStatusException(
                "Cannot cancel a confirmed order.");

        if (order.Status == OrderStatus.Cancelled)
            throw new InvalidOrderStatusException(
                "Order is already cancelled.");

        order.Status = OrderStatus.Cancelled;
        order.CancellationReason = reason;

        await _orderRepository.UpdateAsync(order);
       
        // Save OrderCancelled to Outbox
        var cancelledEvent = new OrderCancelled
        {
            OrderId = order.Id,
            Reason = reason,
            CancelledAt = DateTime.UtcNow
        };

        await _outboxRepository.AddAsync(new OutboxMessage
        {
            EventType = typeof(OrderCancelled).FullName!,
            Payload = JsonSerializer.Serialize(cancelledEvent),
            IsProcessed = false,
            RetryCount = 0
        });

        return MapToDto(order);
    }
    public async Task UpdateOrderStatusAsync(Guid orderId, string status)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
            ?? throw new OrderNotFoundException(orderId);

        if (Enum.TryParse<OrderStatus>(status, out var orderStatus))
        {
            order.Status = orderStatus;
            await _orderRepository.UpdateAsync(order);
        }
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