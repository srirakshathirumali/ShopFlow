using Microsoft.EntityFrameworkCore;
using ShopFlow.NotificationService.Domain.Entities;
using ShopFlow.NotificationService.Domain.Interfaces;

namespace ShopFlow.NotificationService.Infrastructure.Persistence.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Notification notification)
    {
        notification.Id = Guid.NewGuid();
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Notification>> GetByOrderIdAsync(
        Guid orderId) =>
        await _context.Notifications
            .Where(n => n.OrderId == orderId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
}