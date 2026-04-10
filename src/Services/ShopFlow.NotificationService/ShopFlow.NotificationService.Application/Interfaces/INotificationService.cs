using ShopFlow.NotificationService.Application.DTOs;
using ShopFlow.NotificationService.Domain.Enums;

namespace ShopFlow.NotificationService.Application.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(
        Guid orderId,
        Guid customerId,
        NotificationType type,
        string message);

    Task<IEnumerable<NotificationResponseDto>> GetByOrderIdAsync(
        Guid orderId);
}