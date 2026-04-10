using ShopFlow.NotificationService.Application.DTOs;
using ShopFlow.NotificationService.Application.Interfaces;
using ShopFlow.NotificationService.Domain.Entities;
using ShopFlow.NotificationService.Domain.Enums;
using ShopFlow.NotificationService.Domain.Interfaces;

namespace ShopFlow.NotificationService.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(
        INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task SendNotificationAsync(
        Guid orderId,
        Guid customerId,
        NotificationType type,
        string message)
    {
        var notification = new Notification
        {
            OrderId = orderId,
            CustomerId = customerId,
            Type = type,
            Message = message,
            IsSent = true,
            SentAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notification);
    }

    public async Task<IEnumerable<NotificationResponseDto>> GetByOrderIdAsync(
        Guid orderId)
    {
        var notifications = await _notificationRepository
            .GetByOrderIdAsync(orderId);

        return notifications.Select(n => new NotificationResponseDto
        {
            Id = n.Id,
            OrderId = n.OrderId,
            CustomerId = n.CustomerId,
            Type = n.Type,
            Message = n.Message,
            IsSent = n.IsSent,
            SentAt = n.SentAt,
            CreatedAt = n.CreatedAt
        });
    }
}