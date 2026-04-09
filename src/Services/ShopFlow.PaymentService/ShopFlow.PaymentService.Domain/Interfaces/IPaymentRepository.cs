using ShopFlow.PaymentService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.PaymentService.Domain.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByOrderIdAsync(Guid orderId);
        Task AddAsync(Payment payment);
        Task UpdateAsync(Payment payment);
    }
}
