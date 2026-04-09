using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.InventoryService.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity => StockQuantity - ReservedQuantity;
        public decimal Price { get; set; }
    }
}
