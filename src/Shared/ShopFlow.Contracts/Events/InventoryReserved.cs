namespace ShopFlow.Contracts.Events;

public record InventoryReserved
{
    public Guid OrderId { get; init; }
    public List<ReservedItem> Items { get; init; } = new();
    public decimal TotalAmount { get; init; }
    public DateTime ReservedAt { get; init; }
}

public record ReservedItem
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}