namespace ShopFlow.InventoryService.Application.DTOs;

public class ReserveStockRequestDto
{
    public Guid OrderId { get; set; }
    public List<ReserveStockItemDto> Items { get; set; } = new();
}

public class ReserveStockItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}