using Microsoft.AspNetCore.Mvc;
using ShopFlow.NotificationService.Application.DTOs;
using ShopFlow.NotificationService.Application.Interfaces;

namespace ShopFlow.NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>Gets all notifications for an order.</summary>
    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponseDto>),StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotificationsByOrder(Guid orderId)
    {
        var result = await _notificationService
            .GetNotificationsByOrderAsync(orderId);
        return Ok(result);
    }
}