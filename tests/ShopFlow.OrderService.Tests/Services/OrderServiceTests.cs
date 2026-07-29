using FluentAssertions;
using Moq;
using ShopFlow.Contracts.Events;
using ShopFlow.OrderService.Application.DTOs;
using ShopFlow.OrderService.Application.Services;
using ShopFlow.OrderService.Domain.Entities;
using ShopFlow.OrderService.Domain.Enums;
using ShopFlow.OrderService.Domain.Exceptions;
using ShopFlow.OrderService.Domain.Interfaces;
using System.Text.Json;
using Xunit;

namespace ShopFlow.OrderService.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly Mock<IOutboxRepository> _outboxRepositoryMock = new();
    private readonly Application.Services.OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new Application.Services.OrderService(_orderRepositoryMock.Object, _outboxRepositoryMock.Object);
    }

    private static CreateOrderRequestDto BuildCreateOrderRequest(Guid? customerId = null) =>
        new()
        {
            CustomerId = customerId ?? Guid.NewGuid(),
            Items = new List<CreateOrderLineDto>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Widget",
                    Quantity = 2,
                    UnitPrice = 10m
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Gadget",
                    Quantity = 1,
                    UnitPrice = 25m
                }
            }
        };

    private static Order BuildOrder(Guid? id = null, OrderStatus status = OrderStatus.Pending, string? cancellationReason = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Status = status,
            TotalAmount = 45m,
            CancellationReason = cancellationReason,
            CreatedAt = DateTime.UtcNow,
            OrderLines = new List<OrderLine>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = "Widget",
                    Quantity = 2,
                    UnitPrice = 10m
                }
            }
        };

    // ---------- CreateOrderAsync ----------

    [Fact]
    public async Task CreateOrderAsync_WithValidRequest_SavesOrderWithCorrectTotalAndPendingStatus()
    {
        var request = BuildCreateOrderRequest();
        Order? capturedOrder = null;
        _orderRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(o => capturedOrder = o)
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateOrderAsync(request);

        capturedOrder.Should().NotBeNull();
        capturedOrder!.CustomerId.Should().Be(request.CustomerId);
        capturedOrder.Status.Should().Be(OrderStatus.Pending);
        capturedOrder.TotalAmount.Should().Be(45m); // (2 * 10) + (1 * 25)
        capturedOrder.OrderLines.Should().HaveCount(2);

        _orderRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);

        result.Should().NotBeNull();
        result.CustomerId.Should().Be(request.CustomerId);
        result.Status.Should().Be(OrderStatus.Pending);
        result.TotalAmount.Should().Be(45m);
        result.OrderLines.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidRequest_CreatesUnprocessedOutboxMessageWithOrderPlacedEvent()
    {
        var request = BuildCreateOrderRequest();
        OutboxMessage? capturedMessage = null;
        _outboxRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<OutboxMessage>()))
            .Callback<OutboxMessage>(m => capturedMessage = m)
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateOrderAsync(request);

        _outboxRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OutboxMessage>()), Times.Once);

        capturedMessage.Should().NotBeNull();
        capturedMessage!.EventType.Should().Be(typeof(OrderPlaced).FullName);
        capturedMessage.IsProcessed.Should().BeFalse();
        capturedMessage.RetryCount.Should().Be(0);

        var payload = JsonSerializer.Deserialize<OrderPlaced>(capturedMessage.Payload);
        payload.Should().NotBeNull();
        payload!.CustomerId.Should().Be(request.CustomerId);
        payload.TotalAmount.Should().Be(result.TotalAmount);
        payload.Items.Should().HaveCount(request.Items.Count);
    }

    [Fact]
    public async Task CreateOrderAsync_CallsAddAsyncBeforeOutboxAddAsync_InOrder()
    {
        var request = BuildCreateOrderRequest();
        var callOrder = new List<string>();
        _orderRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .Callback(() => callOrder.Add("order"))
            .Returns(Task.CompletedTask);
        _outboxRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<OutboxMessage>()))
            .Callback(() => callOrder.Add("outbox"))
            .Returns(Task.CompletedTask);

        await _sut.CreateOrderAsync(request);

        callOrder.Should().Equal("order", "outbox");
    }

    // ---------- CancelOrderAsync ----------

    [Fact]
    public async Task CancelOrderAsync_WithPendingOrder_CancelsOrderAndPublishesOutboxEvent()
    {
        var order = BuildOrder(status: OrderStatus.Pending);
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _orderRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
        OutboxMessage? capturedMessage = null;
        _outboxRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<OutboxMessage>()))
            .Callback<OutboxMessage>(m => capturedMessage = m)
            .Returns(Task.CompletedTask);

        var result = await _sut.CancelOrderAsync(order.Id, "Customer changed mind");

        result.Status.Should().Be(OrderStatus.Cancelled);
        result.CancellationReason.Should().Be("Customer changed mind");

        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Order>(o =>
            o.Status == OrderStatus.Cancelled && o.CancellationReason == "Customer changed mind")), Times.Once);

        capturedMessage.Should().NotBeNull();
        capturedMessage!.EventType.Should().Be(typeof(OrderCancelled).FullName);
        var payload = JsonSerializer.Deserialize<OrderCancelled>(capturedMessage.Payload);
        payload!.OrderId.Should().Be(order.Id);
        payload.Reason.Should().Be("Customer changed mind");
    }

    [Fact]
    public async Task CancelOrderAsync_WhenOrderNotFound_ThrowsOrderNotFoundException()
    {
        var orderId = Guid.NewGuid();
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync((Order?)null);

        var act = async () => await _sut.CancelOrderAsync(orderId, "reason");

        await act.Should().ThrowAsync<OrderNotFoundException>();
        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
        _outboxRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OutboxMessage>()), Times.Never);
    }

    [Fact]
    public async Task CancelOrderAsync_WhenOrderAlreadyCancelled_ThrowsInvalidOrderStatusException()
    {
        var order = BuildOrder(status: OrderStatus.Cancelled);
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);

        var act = async () => await _sut.CancelOrderAsync(order.Id, "reason");

        await act.Should().ThrowAsync<InvalidOrderStatusException>()
            .WithMessage("Order is already cancelled.");
        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
        _outboxRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OutboxMessage>()), Times.Never);
    }

    [Fact]
    public async Task CancelOrderAsync_WhenOrderConfirmed_ThrowsInvalidOrderStatusException()
    {
        var order = BuildOrder(status: OrderStatus.Confirmed);
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);

        var act = async () => await _sut.CancelOrderAsync(order.Id, "reason");

        await act.Should().ThrowAsync<InvalidOrderStatusException>()
            .WithMessage("Cannot cancel a confirmed order.");
        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
        _outboxRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OutboxMessage>()), Times.Never);
    }

    // ---------- UpdateOrderStatusAsync(Guid, OrderStatus, string?) ----------

    [Fact]
    public async Task UpdateOrderStatusAsync_WithValidOrder_UpdatesStatusAndPersists()
    {
        var order = BuildOrder(status: OrderStatus.Pending);
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _orderRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);

        await _sut.UpdateOrderStatusAsync(order.Id, OrderStatus.InventoryReserved);

        order.Status.Should().Be(OrderStatus.InventoryReserved);
        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Order>(o => o.Status == OrderStatus.InventoryReserved)), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithReason_SetsCancellationReason()
    {
        var order = BuildOrder(status: OrderStatus.Pending);
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _orderRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);

        await _sut.UpdateOrderStatusAsync(order.Id, OrderStatus.Cancelled, "Out of stock");

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancellationReason.Should().Be("Out of stock");
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WhenOrderNotFound_ThrowsOrderNotFoundException()
    {
        var orderId = Guid.NewGuid();
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync((Order?)null);

        var act = async () => await _sut.UpdateOrderStatusAsync(orderId, OrderStatus.Confirmed);

        await act.Should().ThrowAsync<OrderNotFoundException>();
        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }

    // ---------- UpdateOrderStatusAsync(Guid, string) ----------

    [Fact]
    public async Task UpdateOrderStatusAsync_StringOverload_WithValidStatus_UpdatesStatus()
    {
        var order = BuildOrder(status: OrderStatus.Pending);
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _orderRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);

        await _sut.UpdateOrderStatusAsync(order.Id, nameof(OrderStatus.Confirmed));

        order.Status.Should().Be(OrderStatus.Confirmed);
        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_StringOverload_WithInvalidStatus_DoesNotPersist()
    {
        var order = BuildOrder(status: OrderStatus.Pending);
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);

        await _sut.UpdateOrderStatusAsync(order.Id, "NotARealStatus");

        order.Status.Should().Be(OrderStatus.Pending);
        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_StringOverload_WhenOrderNotFound_ThrowsOrderNotFoundException()
    {
        var orderId = Guid.NewGuid();
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync((Order?)null);

        var act = async () => await _sut.UpdateOrderStatusAsync(orderId, nameof(OrderStatus.Confirmed));

        await act.Should().ThrowAsync<OrderNotFoundException>();
    }

    // ---------- GetOrderByIdAsync ----------

    [Fact]
    public async Task GetOrderByIdAsync_WhenOrderExists_ReturnsMappedDto()
    {
        var order = BuildOrder();
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);

        var result = await _sut.GetOrderByIdAsync(order.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(order.Id);
        result.CustomerId.Should().Be(order.CustomerId);
        result.Status.Should().Be(order.Status);
        result.TotalAmount.Should().Be(order.TotalAmount);
        result.OrderLines.Should().HaveCount(order.OrderLines.Count);
        result.OrderLines.First().LineTotal.Should().Be(order.OrderLines.First().LineTotal);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WhenOrderDoesNotExist_ThrowsOrderNotFoundException()
    {
        var orderId = Guid.NewGuid();
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync((Order?)null);

        var act = async () => await _sut.GetOrderByIdAsync(orderId);

        await act.Should().ThrowAsync<OrderNotFoundException>()
            .WithMessage($"*{orderId}*");
    }
}
