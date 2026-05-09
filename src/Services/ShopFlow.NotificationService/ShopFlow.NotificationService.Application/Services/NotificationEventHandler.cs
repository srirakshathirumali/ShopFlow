using Microsoft.Extensions.Logging;
using ShopFlow.Contracts.Events;
using ShopFlow.NotificationService.Application.Interfaces;
using ShopFlow.NotificationService.Domain.Entities;
using ShopFlow.NotificationService.Domain.Enums;
using ShopFlow.NotificationService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.NotificationService.Application.Services
{
    public class NotificationEventHandler : INotificationEventHandler
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationHubService _hubService;
        private readonly ILogger<NotificationEventHandler> _logger;
        public NotificationEventHandler(ILogger<NotificationEventHandler> logger, INotificationRepository notificationRepository, INotificationHubService notificationHubService)
        {
            _logger = logger;
            _notificationRepository = notificationRepository;
            _hubService= notificationHubService;
        }
        public async Task HandleInventoryReservedAsync(InventoryReserved inventoryReserved)
        {
            const string message = "Items reserved. Processing payment...";

            await SaveAndPushAsync(inventoryReserved.OrderId,Guid.Empty,NotificationType.InventoryReserved,message);
        }

        public async Task HandleOrderCancelledAsync(OrderCancelled orderCancelled)
        {
            var message = $"Order cancelled: {orderCancelled.Reason}";

            await SaveAndPushAsync(orderCancelled.OrderId,Guid.Empty,NotificationType.OrderCancelled,message);
        }

        public async Task HandleOrderPlacedAsync(OrderPlaced orderPlaced)
        {
            const string message = "Your order has been placed successfully.";

            await SaveAndPushAsync(orderPlaced.OrderId,orderPlaced.CustomerId,NotificationType.OrderPlaced,message);
        }

        public async Task HandlePaymentFailedAsync(PaymentFailed paymentFailed)
        {
            var message =$"Payment failed: {paymentFailed.Reason}. Order cancelled.";

            await SaveAndPushAsync(paymentFailed.OrderId, Guid.Empty, NotificationType.PaymentFailed, message);
        }

        public async Task HandlePaymentProcessedAsync(PaymentProcessed paymentProcessed)
        {
            const string message = "Payment successful. Your order is confirmed!";

            await SaveAndPushAsync(paymentProcessed.OrderId,Guid.Empty, NotificationType.PaymentProcessed,message);
        }

        private async Task SaveAndPushAsync(Guid orderId,Guid customerId,NotificationType type,string message)
        {
            _logger.LogInformation("Notification — OrderId: {OrderId} Type: {Type} Message: {Message}",orderId, type, message);

            // Save to database
            var notification = new Notification
            {
                OrderId = orderId,
                CustomerId = customerId,
                Type = type,
                Message = message,
                IsSent = false
            };

            await _notificationRepository.AddAsync(notification);

            // Push via SignalR to all clients watching this order
            await _hubService.SendOrderUpdateAsync(orderId.ToString(),type.ToString(),message);

            // Mark as sent
            notification.IsSent = true;
            notification.SentAt = DateTime.UtcNow;
        }
    }
}
