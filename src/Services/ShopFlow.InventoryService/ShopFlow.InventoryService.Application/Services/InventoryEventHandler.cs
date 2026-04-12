using Microsoft.Extensions.Logging;
using ShopFlow.Contracts.Events;
using ShopFlow.InventoryService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.InventoryService.Application.Services
{
    public class InventoryEventHandler : IInventoryEventHandler
    {
        private readonly ILogger<InventoryEventHandler> _logger;
        public InventoryEventHandler(ILogger<InventoryEventHandler> logger)
        {
              _logger = logger;
        }
        public Task HandleOrderCancelledAsync(OrderCancelled orderCancelled)
        {
            _logger.LogInformation("Order cancelled recieved for OrderId:{OrderId}",orderCancelled.OrderId);
            return Task.CompletedTask;
        }

        public Task HandleOrderPlacedAsync(OrderPlaced orderPlaced)
        {
            _logger.LogInformation("Order placed recieved for OrderId:{OrderId}",orderPlaced.OrderId);
            return Task.CompletedTask;
        }

        public Task HandlePaymentFailedAsync(PaymentFailed paymentFailed)
        {
            _logger.LogInformation("Payment failed recieved for OrderId:{OrderId}",paymentFailed.OrderId);
            return Task.CompletedTask;
        }
    }
}
