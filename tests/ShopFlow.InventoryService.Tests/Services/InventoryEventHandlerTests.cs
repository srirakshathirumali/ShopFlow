using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ShopFlow.Contracts.Events;
using ShopFlow.InventoryService.Application.Interfaces;
using ShopFlow.InventoryService.Application.Services;
using ShopFlow.InventoryService.Domain.Entities;
using ShopFlow.InventoryService.Domain.Enums;
using ShopFlow.InventoryService.Domain.Interfaces;
using Xunit;

namespace ShopFlow.InventoryService.Tests.Services;

public class InventoryEventHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IStockReservationRepository> _reservationRepositoryMock = new();
    private readonly Mock<IEventPublisher> _eventPublisherMock = new();
    private readonly Mock<ILogger<InventoryEventHandler>> _loggerMock = new();
    private readonly InventoryEventHandler _sut;

    public InventoryEventHandlerTests()
    {
        _sut = new InventoryEventHandler(
            _productRepositoryMock.Object,
            _reservationRepositoryMock.Object,
            _eventPublisherMock.Object,
            _loggerMock.Object);
    }

    private static Product BuildProduct(Guid? id = null, int stockQuantity = 10, int reservedQuantity = 0, string name = "Widget") =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            SKU = "SKU-001",
            StockQuantity = stockQuantity,
            ReservedQuantity = reservedQuantity,
            Price = 9.99m
        };

    private static OrderItem BuildOrderItem(Guid productId, int quantity, string productName = "Widget") =>
        new()
        {
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = 9.99m
        };

    private static OrderPlaced BuildOrderPlaced(Guid orderId, params OrderItem[] items) =>
        new()
        {
            OrderId = orderId,
            CustomerId = Guid.NewGuid(),
            Items = items.ToList(),
            TotalAmount = items.Sum(i => i.Quantity * i.UnitPrice),
            PlacedAt = DateTime.UtcNow
        };

    private static StockReservation BuildReservation(Guid orderId, Guid productId, int quantity, ReservationStatus status = ReservationStatus.Active) =>
        new()
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            Status = status
        };

    // ---------- HandleOrderPlacedAsync: sufficient stock ----------

    [Fact]
    public async Task HandleOrderPlacedAsync_WithSufficientStock_ReservesStockForEachItem()
    {
        var orderId = Guid.NewGuid();
        var product1 = BuildProduct(stockQuantity: 10, reservedQuantity: 0, name: "Widget");
        var product2 = BuildProduct(stockQuantity: 5, reservedQuantity: 1, name: "Gadget");
        var item1 = BuildOrderItem(product1.Id, 3, "Widget");
        var item2 = BuildOrderItem(product2.Id, 2, "Gadget");
        var orderPlaced = BuildOrderPlaced(orderId, item1, item2);

        _productRepositoryMock.Setup(r => r.GetByIdAsync(product1.Id)).ReturnsAsync(product1);
        _productRepositoryMock.Setup(r => r.GetByIdAsync(product2.Id)).ReturnsAsync(product2);
        _productRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
        _reservationRepositoryMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<StockReservation>>())).Returns(Task.CompletedTask);
        _eventPublisherMock
            .Setup(p => p.PublishAsync(It.IsAny<InventoryReserved>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.HandleOrderPlacedAsync(orderPlaced);

        product1.ReservedQuantity.Should().Be(3);
        product2.ReservedQuantity.Should().Be(3);

        _productRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.Id == product1.Id && p.ReservedQuantity == 3)), Times.Once);
        _productRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.Id == product2.Id && p.ReservedQuantity == 3)), Times.Once);

        _reservationRepositoryMock.Verify(r => r.AddRangeAsync(It.Is<IEnumerable<StockReservation>>(list =>
            list.Count() == 2 &&
            list.Any(rr => rr.ProductId == product1.Id && rr.Quantity == 3 && rr.OrderId == orderId && rr.Status == ReservationStatus.Active) &&
            list.Any(rr => rr.ProductId == product2.Id && rr.Quantity == 2 && rr.OrderId == orderId && rr.Status == ReservationStatus.Active))),
            Times.Once);
    }

    [Fact]
    public async Task HandleOrderPlacedAsync_WithSufficientStock_PublishesInventoryReservedWithCorrectItems()
    {
        var orderId = Guid.NewGuid();
        var product = BuildProduct(stockQuantity: 10, reservedQuantity: 0);
        var item = BuildOrderItem(product.Id, 4);
        var orderPlaced = BuildOrderPlaced(orderId, item);

        _productRepositoryMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _productRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
        _reservationRepositoryMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<StockReservation>>())).Returns(Task.CompletedTask);

        InventoryReserved? published = null;
        _eventPublisherMock
            .Setup(p => p.PublishAsync(It.IsAny<InventoryReserved>(), It.IsAny<CancellationToken>()))
            .Callback<InventoryReserved, CancellationToken>((e, _) => published = e)
            .Returns(Task.CompletedTask);

        await _sut.HandleOrderPlacedAsync(orderPlaced);

        published.Should().NotBeNull();
        published!.OrderId.Should().Be(orderId);
        published.Items.Should().ContainSingle(i => i.ProductId == product.Id && i.Quantity == 4);

        _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<InventoryReservationFailed>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- HandleOrderPlacedAsync: insufficient stock ----------

    [Fact]
    public async Task HandleOrderPlacedAsync_WithInsufficientStock_PublishesInventoryReservationFailed()
    {
        var orderId = Guid.NewGuid();
        var product = BuildProduct(stockQuantity: 5, reservedQuantity: 3, name: "Widget"); // available = 2
        var item = BuildOrderItem(product.Id, 10);
        var orderPlaced = BuildOrderPlaced(orderId, item);

        _productRepositoryMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);

        InventoryReservationFailed? published = null;
        _eventPublisherMock
            .Setup(p => p.PublishAsync(It.IsAny<InventoryReservationFailed>(), It.IsAny<CancellationToken>()))
            .Callback<InventoryReservationFailed, CancellationToken>((e, _) => published = e)
            .Returns(Task.CompletedTask);

        await _sut.HandleOrderPlacedAsync(orderPlaced);

        published.Should().NotBeNull();
        published!.OrderId.Should().Be(orderId);
        published.Reason.Should().Contain("Insufficient stock").And.Contain("Widget");

        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        _reservationRepositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<StockReservation>>()), Times.Never);
        _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<InventoryReserved>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- HandleOrderPlacedAsync: product not found ----------

    [Fact]
    public async Task HandleOrderPlacedAsync_WhenProductNotFound_PublishesInventoryReservationFailed()
    {
        var orderId = Guid.NewGuid();
        var missingProductId = Guid.NewGuid();
        var item = BuildOrderItem(missingProductId, 1);
        var orderPlaced = BuildOrderPlaced(orderId, item);

        _productRepositoryMock.Setup(r => r.GetByIdAsync(missingProductId)).ReturnsAsync((Product?)null);

        InventoryReservationFailed? published = null;
        _eventPublisherMock
            .Setup(p => p.PublishAsync(It.IsAny<InventoryReservationFailed>(), It.IsAny<CancellationToken>()))
            .Callback<InventoryReservationFailed, CancellationToken>((e, _) => published = e)
            .Returns(Task.CompletedTask);

        await _sut.HandleOrderPlacedAsync(orderPlaced);

        published.Should().NotBeNull();
        published!.OrderId.Should().Be(orderId);
        published.Reason.Should().Contain(missingProductId.ToString());
        published.Reason.Should().Contain("not found");

        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        _reservationRepositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<StockReservation>>()), Times.Never);
        _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<InventoryReserved>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- HandlePaymentFailedAsync ----------

    [Fact]
    public async Task HandlePaymentFailedAsync_ReleasesStockAndUpdatesReservations()
    {
        var orderId = Guid.NewGuid();
        var product = BuildProduct(stockQuantity: 10, reservedQuantity: 4);
        var reservation = BuildReservation(orderId, product.Id, 4, ReservationStatus.Active);

        _reservationRepositoryMock.Setup(r => r.GetByOrderIdAsync(orderId)).ReturnsAsync(new[] { reservation });
        _productRepositoryMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _productRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
        _reservationRepositoryMock.Setup(r => r.UpdateRangeAsync(It.IsAny<IEnumerable<StockReservation>>())).Returns(Task.CompletedTask);

        var paymentFailed = new PaymentFailed { OrderId = orderId, Reason = "Card declined", FailedAt = DateTime.UtcNow };

        await _sut.HandlePaymentFailedAsync(paymentFailed);

        product.ReservedQuantity.Should().Be(0);
        reservation.Status.Should().Be(ReservationStatus.Released);
        reservation.ReleasedAt.Should().NotBeNull();

        _productRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.Id == product.Id && p.ReservedQuantity == 0)), Times.Once);
        _reservationRepositoryMock.Verify(r => r.UpdateRangeAsync(It.Is<IEnumerable<StockReservation>>(list =>
            list.Count() == 1 && list.First().Status == ReservationStatus.Released)), Times.Once);
    }

    // ---------- HandleOrderCancelledAsync ----------

    [Fact]
    public async Task HandleOrderCancelledAsync_ReleasesStock()
    {
        var orderId = Guid.NewGuid();
        var product = BuildProduct(stockQuantity: 20, reservedQuantity: 7);
        var reservation = BuildReservation(orderId, product.Id, 7, ReservationStatus.Active);

        _reservationRepositoryMock.Setup(r => r.GetByOrderIdAsync(orderId)).ReturnsAsync(new[] { reservation });
        _productRepositoryMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _productRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
        _reservationRepositoryMock.Setup(r => r.UpdateRangeAsync(It.IsAny<IEnumerable<StockReservation>>())).Returns(Task.CompletedTask);

        var orderCancelled = new OrderCancelled { OrderId = orderId, Reason = "Customer request", CancelledAt = DateTime.UtcNow };

        await _sut.HandleOrderCancelledAsync(orderCancelled);

        product.ReservedQuantity.Should().Be(0);
        reservation.Status.Should().Be(ReservationStatus.Released);

        _productRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.Id == product.Id && p.ReservedQuantity == 0)), Times.Once);
        _reservationRepositoryMock.Verify(r => r.UpdateRangeAsync(It.IsAny<IEnumerable<StockReservation>>()), Times.Once);
    }

    [Fact]
    public async Task HandleOrderCancelledAsync_SkipsReservationsThatAreNotActive()
    {
        var orderId = Guid.NewGuid();
        var product = BuildProduct(stockQuantity: 20, reservedQuantity: 5);
        var releasedReservation = BuildReservation(orderId, product.Id, 5, ReservationStatus.Released);

        _reservationRepositoryMock.Setup(r => r.GetByOrderIdAsync(orderId)).ReturnsAsync(new[] { releasedReservation });
        _reservationRepositoryMock.Setup(r => r.UpdateRangeAsync(It.IsAny<IEnumerable<StockReservation>>())).Returns(Task.CompletedTask);

        var orderCancelled = new OrderCancelled { OrderId = orderId, Reason = "n/a", CancelledAt = DateTime.UtcNow };

        await _sut.HandleOrderCancelledAsync(orderCancelled);

        // Reservation was already Released (not Active), so the release loop never touches the product.
        product.ReservedQuantity.Should().Be(5); // untouched
        releasedReservation.ReleasedAt.Should().BeNull(); // untouched, loop never processed it
        _productRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }
}
