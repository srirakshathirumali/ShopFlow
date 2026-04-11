using ShopFlow.InventoryService.Application.DTOs;

namespace ShopFlow.InventoryService.Application.Interfaces;

public interface IInventoryService
{
    Task<ProductResponseDto> CreateProductAsync(CreateProductRequestDto request);
    Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync();
    Task<ProductResponseDto> GetProductByIdAsync(Guid productId);
    Task ReserveStockAsync(ReserveStockRequestDto request);
    Task ReleaseStockAsync(Guid orderId);
    Task ConfirmStockAsync(Guid orderId);
    Task<ProductResponseDto> UpdateStockAsync(Guid productId, int quantity);
}