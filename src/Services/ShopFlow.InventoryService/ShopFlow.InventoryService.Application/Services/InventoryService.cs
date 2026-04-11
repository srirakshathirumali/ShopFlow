using ShopFlow.InventoryService.Application.DTOs;
using ShopFlow.InventoryService.Application.Interfaces;
using ShopFlow.InventoryService.Domain.Entities;
using ShopFlow.InventoryService.Domain.Enums;
using ShopFlow.InventoryService.Domain.Exceptions;
using ShopFlow.InventoryService.Domain.Interfaces;

namespace ShopFlow.InventoryService.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IProductRepository _productRepository;
    private readonly IStockReservationRepository _reservationRepository;

    public InventoryService(
        IProductRepository productRepository,
        IStockReservationRepository reservationRepository)
    {
        _productRepository = productRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<ProductResponseDto> CreateProductAsync(CreateProductRequestDto request)
    {
        var product = new Product
        {
            Name = request.Name,
            SKU = request.SKU,
            StockQuantity = request.InitialStock,
            Price = request.Price
        };

        await _productRepository.AddAsync(product);
        return MapToDto(product);
    }

    public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(MapToDto);
    }

    public async Task<ProductResponseDto> GetProductByIdAsync(Guid productId)
    {
        var product = await _productRepository.GetByIdAsync(productId)
            ?? throw new ProductNotFoundException(productId);

        return MapToDto(product);
    }

    public async Task ReserveStockAsync(ReserveStockRequestDto request)
    {
        var reservations = new List<StockReservation>();

        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId)
                ?? throw new ProductNotFoundException(item.ProductId);

            if (product.AvailableQuantity < item.Quantity)
                throw new InsufficientStockException(
                    item.ProductId,
                    item.Quantity,
                    product.AvailableQuantity);

            product.ReservedQuantity += item.Quantity;
            await _productRepository.UpdateAsync(product);

            reservations.Add(new StockReservation
            {
                OrderId = request.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Status = ReservationStatus.Active
            });
        }

        await _reservationRepository.AddRangeAsync(reservations);
    }

    public async Task ReleaseStockAsync(Guid orderId)
    {
        var reservations = (await _reservationRepository
            .GetByOrderIdAsync(orderId))
            .Where(r => r.Status == ReservationStatus.Active)
            .ToList();

        foreach (var reservation in reservations)
        {
            reservation.Product.ReservedQuantity -= reservation.Quantity;
            reservation.Status = ReservationStatus.Released;
            reservation.ReleasedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(reservation.Product);
        }

        await _reservationRepository.UpdateRangeAsync(reservations);
    }

    public async Task ConfirmStockAsync(Guid orderId)
    {
        var reservations = (await _reservationRepository
            .GetByOrderIdAsync(orderId))
            .Where(r => r.Status == ReservationStatus.Active)
            .ToList();

        foreach (var reservation in reservations)
        {
            reservation.Product.StockQuantity -= reservation.Quantity;
            reservation.Product.ReservedQuantity -= reservation.Quantity;
            reservation.Status = ReservationStatus.Confirmed;
            await _productRepository.UpdateAsync(reservation.Product);
        }

        await _reservationRepository.UpdateRangeAsync(reservations);
    }

    private static ProductResponseDto MapToDto(Product product) =>
        new()
        {
            Id = product.Id,
            Name = product.Name,
            SKU = product.SKU,
            StockQuantity = product.StockQuantity,
            ReservedQuantity = product.ReservedQuantity,
            AvailableQuantity = product.AvailableQuantity,
            Price = product.Price
        };

    public async Task<ProductResponseDto> UpdateStockAsync(Guid productId, int quantity)
    {
        var product = await _productRepository.GetByIdAsync(productId)
            ?? throw new ProductNotFoundException(productId);

        if (quantity < 0)
            throw new ArgumentException(
                "Stock quantity cannot be negative.");

        product.StockQuantity = quantity;
        await _productRepository.UpdateAsync(product);
        return MapToDto(product);
    }
}