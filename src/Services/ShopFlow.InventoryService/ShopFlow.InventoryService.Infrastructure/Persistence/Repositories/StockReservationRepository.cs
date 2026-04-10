using Microsoft.EntityFrameworkCore;
using ShopFlow.InventoryService.Domain.Entities;
using ShopFlow.InventoryService.Domain.Interfaces;

namespace ShopFlow.InventoryService.Infrastructure.Persistence.Repositories;

public class StockReservationRepository : IStockReservationRepository
{
    private readonly InventoryDbContext _context;

    public StockReservationRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StockReservation>> GetByOrderIdAsync(
        Guid orderId) =>
        await _context.StockReservations
            .Include(r => r.Product)
            .Where(r => r.OrderId == orderId)
            .ToListAsync();

    public async Task AddRangeAsync(
        IEnumerable<StockReservation> reservations)
    {
        foreach (var reservation in reservations)
            reservation.Id = Guid.NewGuid();

        await _context.StockReservations.AddRangeAsync(reservations);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(
        IEnumerable<StockReservation> reservations)
    {
        _context.StockReservations.UpdateRange(reservations);
        await _context.SaveChangesAsync();
    }
}