using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ShopFlow.Contracts.Events;
using ShopFlow.PaymentService.Application.Interfaces;
using ShopFlow.PaymentService.Application.Services;
using ShopFlow.PaymentService.Domain.Entities;
using ShopFlow.PaymentService.Domain.Enums;
using ShopFlow.PaymentService.Domain.Interfaces;
using Xunit;

namespace ShopFlow.PaymentService.Tests.Services;

public class PaymentEventHandlerTests
{
    private readonly Mock<ILogger<PaymentEventHandler>> _loggerMock = new();
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock = new();
    private readonly Mock<IEventPublisher> _eventPublisherMock = new();
    private readonly Mock<IPaymentGateway> _paymentGatewayMock = new();
    private readonly PaymentEventHandler _sut;

    public PaymentEventHandlerTests()
    {
        _sut = new PaymentEventHandler(
            _loggerMock.Object,
            _paymentRepositoryMock.Object,
            _eventPublisherMock.Object,
            _paymentGatewayMock.Object);
    }

    private static InventoryReserved BuildInventoryReserved(Guid? orderId = null) =>
        new()
        {
            OrderId = orderId ?? Guid.NewGuid(),
            Items = new List<ReservedItem>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 2 }
            },
            ReservedAt = DateTime.UtcNow
        };

    [Fact]
    public async Task HandleInventoryReservedAsync_WhenGatewaySucceeds_PublishesPaymentProcessed()
    {
        var inventoryReserved = BuildInventoryReserved();
        _paymentRepositoryMock.Setup(r => r.GetByOrderIdAsync(inventoryReserved.OrderId)).ReturnsAsync((Payment?)null);
        _paymentGatewayMock
            .Setup(g => g.ProcessAsync(It.IsAny<PaymentGatewayRequest>()))
            .ReturnsAsync(new PaymentGatewayResponse { IsSuccess = true, TransactionId = "txn-1" });

        await _sut.HandleInventoryReservedAsync(inventoryReserved);

        _eventPublisherMock.Verify(p => p.PublishAsync(
            It.Is<PaymentProcessed>(e => e.OrderId == inventoryReserved.OrderId && e.Amount == 999.99m),
            It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<PaymentFailed>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleInventoryReservedAsync_WhenGatewayFails_PublishesPaymentFailed()
    {
        var inventoryReserved = BuildInventoryReserved();
        _paymentRepositoryMock.Setup(r => r.GetByOrderIdAsync(inventoryReserved.OrderId)).ReturnsAsync((Payment?)null);
        _paymentGatewayMock
            .Setup(g => g.ProcessAsync(It.IsAny<PaymentGatewayRequest>()))
            .ReturnsAsync(new PaymentGatewayResponse { IsSuccess = false, FailureReason = "Card declined by issuing bank." });

        await _sut.HandleInventoryReservedAsync(inventoryReserved);

        _eventPublisherMock.Verify(p => p.PublishAsync(
            It.Is<PaymentFailed>(e => e.OrderId == inventoryReserved.OrderId && e.Reason == "Card declined by issuing bank."),
            It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<PaymentProcessed>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleInventoryReservedAsync_WhenPaymentAlreadyExistsForOrder_SkipsProcessing()
    {
        var inventoryReserved = BuildInventoryReserved();
        var existingPayment = new Payment { OrderId = inventoryReserved.OrderId, Status = PaymentStatus.Processed };
        _paymentRepositoryMock.Setup(r => r.GetByOrderIdAsync(inventoryReserved.OrderId)).ReturnsAsync(existingPayment);

        await _sut.HandleInventoryReservedAsync(inventoryReserved);

        _paymentGatewayMock.Verify(g => g.ProcessAsync(It.IsAny<PaymentGatewayRequest>()), Times.Never);
        _paymentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Never);
        _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<PaymentProcessed>(), It.IsAny<CancellationToken>()), Times.Never);
        _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<PaymentFailed>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleInventoryReservedAsync_SavesPaymentToRepositoryWithGatewayOutcome()
    {
        var inventoryReserved = BuildInventoryReserved();
        _paymentRepositoryMock.Setup(r => r.GetByOrderIdAsync(inventoryReserved.OrderId)).ReturnsAsync((Payment?)null);
        _paymentGatewayMock
            .Setup(g => g.ProcessAsync(It.IsAny<PaymentGatewayRequest>()))
            .ReturnsAsync(new PaymentGatewayResponse { IsSuccess = true, TransactionId = "txn-2" });

        Payment? capturedPayment = null;
        _paymentRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Payment>()))
            .Callback<Payment>(p => capturedPayment = p)
            .Returns(Task.CompletedTask);

        await _sut.HandleInventoryReservedAsync(inventoryReserved);

        _paymentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Once);
        capturedPayment.Should().NotBeNull();
        capturedPayment!.OrderId.Should().Be(inventoryReserved.OrderId);
        capturedPayment.Amount.Should().Be(999.99m);
        capturedPayment.Status.Should().Be(PaymentStatus.Processed);
        capturedPayment.ProcessedAt.Should().NotBeNull();
    }
}
