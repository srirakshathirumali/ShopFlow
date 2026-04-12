using Microsoft.Extensions.Logging;
using ShopFlow.Contracts.Events;
using ShopFlow.NotificationService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.NotificationService.Application.Services
{
    public class NotificationEventHandler : INotificationEventHandler
    {
        private readonly ILogger<NotificationEventHandler> _logger;
        public NotificationEventHandler(ILogger<NotificationEventHandler> logger)
        {
            _logger = logger;
        }
        public Task HandleInventoryReservedAsync(InventoryReserved inventoryReserved)
        {
            _logger.LogInformation("Inventory reserved for OrderId:{OrderId}", inventoryReserved.OrderId);
            return Task.CompletedTask;
        }

        public Task HandleOrderCancelledAsync(OrderCancelled orderCancelled)
        {
            _logger.LogInformation("Order cancelled for OrderId:{OrderId}", orderCancelled.OrderId);
            return Task.CompletedTask;
        }

        public Task HandleOrderPlacedAsync(OrderPlaced orderPlaced)
        {
            _logger.LogInformation("Order placed for OrderId:{OrderId}", orderPlaced.OrderId);
            return Task.CompletedTask;
        }

        public Task HandlePaymentFailedAsync(PaymentFailed paymentFailed)
        {
            _logger.LogInformation("Payment failed for OrderId:{OrderId}", paymentFailed.OrderId);
            return Task.CompletedTask;
        }

        public Task HandlePaymentProcessedAsync(PaymentProcessed paymentProcessed)
        {
            _logger.LogInformation("Payment processed for OrderId:{OrderId}", paymentProcessed.OrderId);
            return Task.CompletedTask;
        }
    }
}
