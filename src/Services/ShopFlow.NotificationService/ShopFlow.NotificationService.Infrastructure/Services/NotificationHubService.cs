using Microsoft.AspNetCore.SignalR;
using ShopFlow.NotificationService.Application.Interfaces;
using ShopFlow.NotificationService.Infrastructure.Hubs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.NotificationService.Infrastructure.Services
{
    public class NotificationHubService : INotificationHubService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        public NotificationHubService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task SendOrderUpdateAsync(string orderId, string status, string message)
        {
           // Send to all clients in the order group
              await _hubContext.Clients.Group($"order-{orderId}").SendAsync("OrderStatusUpdated", new
            {
                orderId,
                status,
                message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
