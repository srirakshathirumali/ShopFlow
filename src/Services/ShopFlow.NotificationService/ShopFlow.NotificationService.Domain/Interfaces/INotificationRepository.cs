using ShopFlow.NotificationService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.NotificationService.Domain.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<IEnumerable<Notification>> GetByOrderIdAsync(Guid orderId);
    }
}
