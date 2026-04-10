using ShopFlow.NotificationService.Domain.Enums;

namespace ShopFlow.NotificationService.Application.DTOs;

public class NotificationResponseDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}