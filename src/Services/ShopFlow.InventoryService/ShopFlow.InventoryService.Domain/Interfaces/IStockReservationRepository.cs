using ShopFlow.InventoryService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.InventoryService.Domain.Interfaces
{
    public interface IStockReservationRepository
    {
        Task<IEnumerable<StockReservation>> GetByOrderIdAsync(Guid orderId);
        Task AddRangeAsync(IEnumerable<StockReservation> reservations);
        Task UpdateRangeAsync(IEnumerable<StockReservation> reservations);
    }
}
