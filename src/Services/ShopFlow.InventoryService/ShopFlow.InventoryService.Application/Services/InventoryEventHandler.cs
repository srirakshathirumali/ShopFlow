using Microsoft.Extensions.Logging;
using ShopFlow.Contracts.Events;
using ShopFlow.InventoryService.Application.Interfaces;
using ShopFlow.InventoryService.Domain.Entities;
using ShopFlow.InventoryService.Domain.Enums;
using ShopFlow.InventoryService.Domain.Exceptions;
using ShopFlow.InventoryService.Domain.Interfaces;

namespace ShopFlow.InventoryService.Application.Services;

public class InventoryEventHandler : IInventoryEventHandler
{
    private readonly IProductRepository _productRepository;
    private readonly IStockReservationRepository _reservationRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<InventoryEventHandler> _logger;

    public InventoryEventHandler(
        IProductRepository productRepository,
        IStockReservationRepository reservationRepository,
        IEventPublisher eventPublisher,
        ILogger<InventoryEventHandler> logger)
    {
        _productRepository = productRepository;
        _reservationRepository = reservationRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task HandleOrderPlacedAsync(OrderPlaced orderPlaced)
    {
        _logger.LogInformation(
            "Processing OrderPlaced for OrderId: {OrderId}",
            orderPlaced.OrderId);

        try
        {
            var existing = await _reservationRepository.GetByOrderIdAsync(orderPlaced.OrderId);
            if (existing.Any())
            {
                _logger.LogWarning(
                    "Duplicate OrderPlaced received for OrderId {OrderId} — skipping",
                    orderPlaced.OrderId);
                return;
            }

            var itemsWithProducts = new List<(OrderItem Item, Product Product)>();

            // Pass 1 — validate every item before reserving anything
            foreach (var item in orderPlaced.Items)
            {
                var product = await _productRepository
                    .GetByIdAsync(item.ProductId);

                if (product is null)
                {
                    await PublishReservationFailedAsync(orderPlaced.OrderId, $"Product '{item.ProductId}' not found.");
                    return;
                }

                if (product.AvailableQuantity < item.Quantity)
                {
                    await PublishReservationFailedAsync(
                        orderPlaced.OrderId,
                        $"Insufficient stock for '{product.Name}'. " +
                        $"Requested: {item.Quantity}, " +
                        $"Available: {product.AvailableQuantity}.");
                    return;
                }

                itemsWithProducts.Add((item, product));
            }

            // Pass 2 — all items validated, safe to reserve
            var reservations = new List<StockReservation>();

            foreach (var (item, product) in itemsWithProducts)
            {
                product.ReservedQuantity += item.Quantity;
                await _productRepository.UpdateAsync(product);

                reservations.Add(new StockReservation
                {
                    OrderId = orderPlaced.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Status = ReservationStatus.Active
                });
            }

            // Save all reservations
            await _reservationRepository.AddRangeAsync(reservations);

            // Publish InventoryReserved event
            await _eventPublisher.PublishAsync(new InventoryReserved
            {
                OrderId = orderPlaced.OrderId,
                ReservedAt = DateTime.UtcNow,
                TotalAmount = orderPlaced.TotalAmount,
                Items = reservations.Select(r => new ReservedItem
                {
                    ProductId = r.ProductId,
                    Quantity = r.Quantity
                }).ToList()
            });

            _logger.LogInformation("Stock reserved successfully for OrderId: {OrderId}",orderPlaced.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Error reserving stock for OrderId: {OrderId}",orderPlaced.OrderId);

            await PublishReservationFailedAsync(orderPlaced.OrderId,"Unexpected error during stock reservation.");
        }
    }

    public async Task HandleOrderCancelledAsync(OrderCancelled orderCancelled)
    {
        _logger.LogInformation("Releasing stock for cancelled OrderId: {OrderId}",orderCancelled.OrderId);

        await ReleaseStockAsync(orderCancelled.OrderId);
    }

    public async Task HandlePaymentFailedAsync(PaymentFailed paymentFailed)
    {
        _logger.LogInformation( "Releasing stock for failed payment OrderId: {OrderId}",paymentFailed.OrderId);

        await ReleaseStockAsync(paymentFailed.OrderId);
    }

    // ── Helpers ────────────────────────────────────────────

    private async Task ReleaseStockAsync(Guid orderId)
    {
        var reservations = await _reservationRepository
            .GetByOrderIdAsync(orderId);

        foreach (var reservation in reservations
            .Where(r => r.Status == ReservationStatus.Active))
        {
            var product = await _productRepository
                .GetByIdAsync(reservation.ProductId);

            if (product is null) continue;

            product.ReservedQuantity -= reservation.Quantity;
            reservation.Status = ReservationStatus.Released;
            reservation.ReleasedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
        }

        await _reservationRepository.UpdateRangeAsync(
            reservations.Where(r => r.Status == ReservationStatus.Released)
                        .ToList());

        _logger.LogInformation("Stock released for OrderId: {OrderId}", orderId);
    }

    private async Task PublishReservationFailedAsync(Guid orderId, string reason)
    {
        _logger.LogWarning(
            "Reservation failed for OrderId: {OrderId}. Reason: {Reason}",
            orderId, reason);

        await _eventPublisher.PublishAsync(new InventoryReservationFailed
        {
            OrderId = orderId,
            Reason = reason,
            FailedAt = DateTime.UtcNow
        });
    }
}