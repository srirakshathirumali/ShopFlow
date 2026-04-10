namespace ShopFlow.InventoryService.Application.DTOs;

public class CreateProductRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int InitialStock { get; set; }
    public decimal Price { get; set; }
}