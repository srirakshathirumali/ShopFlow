using Microsoft.Extensions.Logging;
using ShopFlow.Contracts.Events;
using ShopFlow.PaymentService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.PaymentService.Application.Services
{
    public class PaymentEventHandler : IPaymentEventHandler
    {
        private readonly ILogger<PaymentEventHandler> _logger;
        public PaymentEventHandler(ILogger<PaymentEventHandler> logger)
        {
            _logger = logger;
        }
        public Task HandleInventoryReservedAsync(InventoryReserved inventoryReserved)
        {
            _logger.LogInformation("InventoryReserved for OrderId:{OrderId}", inventoryReserved.OrderId);
            return Task.CompletedTask;
        }
    }
}
