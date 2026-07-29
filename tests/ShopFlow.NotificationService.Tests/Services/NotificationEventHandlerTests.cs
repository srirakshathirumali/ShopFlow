using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ShopFlow.Contracts.Events;
using ShopFlow.NotificationService.Application.Interfaces;
using ShopFlow.NotificationService.Application.Services;
using ShopFlow.NotificationService.Domain.Entities;
using ShopFlow.NotificationService.Domain.Enums;
using ShopFlow.NotificationService.Domain.Interfaces;
using Xunit;

namespace ShopFlow.NotificationService.Tests.Services;

public class NotificationEventHandlerTests
{
    private readonly Mock<ILogger<NotificationEventHandler>> _loggerMock = new();
    private readonly Mock<INotificationRepository> _notificationRepositoryMock = new();
    private readonly Mock<INotificationHubService> _hubServiceMock = new();
    private readonly NotificationEventHandler _sut;

    public NotificationEventHandlerTests()
    {
        _sut = new NotificationEventHandler(
            _loggerMock.Object,
            _notificationRepositoryMock.Object,
            _hubServiceMock.Object);
    }

    private static OrderPlaced BuildOrderPlaced(Guid? orderId = null, Guid? customerId = null) =>
        new()
        {
            OrderId = orderId ?? Guid.NewGuid(),
            CustomerId = customerId ?? Guid.NewGuid(),
            Items = new List<OrderItem>
            {
                new() { ProductId = Guid.NewGuid(), ProductName = "Widget", Quantity = 1, UnitPrice = 10m }
            },
            TotalAmount = 10m,
            PlacedAt = DateTime.UtcNow
        };

    // ---------- HandleOrderPlacedAsync ----------

    [Fact]
    public async Task HandleOrderPlacedAsync_SavesNotificationToRepository()
    {
        var orderPlaced = BuildOrderPlaced();
        Notification? captured = null;
        _notificationRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .Callback<Notification>(n => captured = n)
            .Returns(Task.CompletedTask);

        await _sut.HandleOrderPlacedAsync(orderPlaced);

        _notificationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Once);
        captured.Should().NotBeNull();
        captured!.OrderId.Should().Be(orderPlaced.OrderId);
        captured.CustomerId.Should().Be(orderPlaced.CustomerId);
        captured.Type.Should().Be(NotificationType.OrderPlaced);
        captured.Message.Should().Be("Your order has been placed successfully.");
    }

    [Fact]
    public async Task HandleOrderPlacedAsync_PushesSignalRUpdateWithCorrectMessage()
    {
        var orderPlaced = BuildOrderPlaced();

        await _sut.HandleOrderPlacedAsync(orderPlaced);

        _hubServiceMock.Verify(h => h.SendOrderUpdateAsync(
            orderPlaced.OrderId.ToString(),
            NotificationType.OrderPlaced.ToString(),
            "Your order has been placed successfully."), Times.Once);
    }

    // ---------- HandlePaymentProcessedAsync ----------

    [Fact]
    public async Task HandlePaymentProcessedAsync_SavesNotificationWithPaymentProcessedType()
    {
        var paymentProcessed = new PaymentProcessed
        {
            OrderId = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            Amount = 999.99m,
            ProcessedAt = DateTime.UtcNow
        };
        Notification? captured = null;
        _notificationRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .Callback<Notification>(n => captured = n)
            .Returns(Task.CompletedTask);

        await _sut.HandlePaymentProcessedAsync(paymentProcessed);

        captured.Should().NotBeNull();
        captured!.OrderId.Should().Be(paymentProcessed.OrderId);
        captured.Type.Should().Be(NotificationType.PaymentProcessed);
        captured.Message.Should().Be("Payment successful. Your order is confirmed!");
    }

    // ---------- HandlePaymentFailedAsync ----------

    [Fact]
    public async Task HandlePaymentFailedAsync_PushesFailureMessageWithReason()
    {
        var paymentFailed = new PaymentFailed
        {
            OrderId = Guid.NewGuid(),
            Reason = "Card declined by issuing bank.",
            FailedAt = DateTime.UtcNow
        };

        await _sut.HandlePaymentFailedAsync(paymentFailed);

        var expectedMessage = "Payment failed: Card declined by issuing bank.. Order cancelled.";
        _hubServiceMock.Verify(h => h.SendOrderUpdateAsync(
            paymentFailed.OrderId.ToString(),
            NotificationType.PaymentFailed.ToString(),
            expectedMessage), Times.Once);
        _notificationRepositoryMock.Verify(r => r.AddAsync(It.Is<Notification>(n =>
            n.Type == NotificationType.PaymentFailed && n.Message == expectedMessage)), Times.Once);
    }

    // ---------- HandleOrderCancelledAsync ----------

    [Fact]
    public async Task HandleOrderCancelledAsync_IncludesCancellationReasonInMessage()
    {
        var orderCancelled = new OrderCancelled
        {
            OrderId = Guid.NewGuid(),
            Reason = "Out of stock",
            CancelledAt = DateTime.UtcNow
        };
        Notification? captured = null;
        _notificationRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .Callback<Notification>(n => captured = n)
            .Returns(Task.CompletedTask);

        await _sut.HandleOrderCancelledAsync(orderCancelled);

        var expectedMessage = "Order cancelled: Out of stock";
        captured.Should().NotBeNull();
        captured!.Type.Should().Be(NotificationType.OrderCancelled);
        captured.Message.Should().Be(expectedMessage);

        _hubServiceMock.Verify(h => h.SendOrderUpdateAsync(
            orderCancelled.OrderId.ToString(),
            NotificationType.OrderCancelled.ToString(),
            expectedMessage), Times.Once);
    }
}
