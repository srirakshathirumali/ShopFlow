using Microsoft.EntityFrameworkCore;
using ShopFlow.OrderService.Domain.Entities;
using ShopFlow.OrderService.Domain.Interfaces;

namespace ShopFlow.OrderService.Infrastructure.Persistence.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly OrderDbContext _context;

    public OutboxRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(OutboxMessage message)
    {
        message.Id = Guid.NewGuid();
        await _context.OutboxMessages.AddAsync(message);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<OutboxMessage>> GetUnprocessedAsync() =>
        await _context.OutboxMessages
            .Where(m => !m.IsProcessed && m.RetryCount < 5)
            .OrderBy(m => m.CreatedAt)
            .Take(50)
            .ToListAsync();

    public async Task MarkAsProcessedAsync(Guid messageId)
    {
        var message = await _context.OutboxMessages.FindAsync(messageId);
        if (message is null) return;

        message.IsProcessed = true;
        message.ProcessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task IncrementRetryCountAsync(Guid messageId)
    {
        var message = await _context.OutboxMessages.FindAsync(messageId);
        if (message is null) return;

        message.RetryCount++;
        await _context.SaveChangesAsync();
    }
}