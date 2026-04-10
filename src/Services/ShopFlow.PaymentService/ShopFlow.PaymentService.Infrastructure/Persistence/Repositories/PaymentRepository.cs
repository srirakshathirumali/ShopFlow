using Microsoft.EntityFrameworkCore;
using ShopFlow.PaymentService.Domain.Entities;
using ShopFlow.PaymentService.Domain.Interfaces;

namespace ShopFlow.PaymentService.Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    public PaymentRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByOrderIdAsync(Guid orderId) =>
        await _context.Payments
            .FirstOrDefaultAsync(p => p.OrderId == orderId);

    public async Task AddAsync(Payment payment)
    {
        payment.Id = Guid.NewGuid();
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
    }
}