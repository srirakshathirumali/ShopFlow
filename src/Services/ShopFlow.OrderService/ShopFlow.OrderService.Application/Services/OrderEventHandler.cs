using Microsoft.Extensions.Logging;
using ShopFlow.Contracts.Events;
using ShopFlow.OrderService.Application.Interfaces;
using ShopFlow.OrderService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.OrderService.Application.Services
{
    public class OrderEventHandler : IOrderEventHandler
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderEventHandler> _logger;
        public OrderEventHandler(IOrderService orderService, ILogger<OrderEventHandler> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }
        public async Task HandleInventoryReservationFailedAsync(InventoryReservationFailed inventoryReservationFailed)
        {
            _logger.LogInformation("Inventory Reservation falied for OrderId:{OrderId}. Reason :{Reason}", inventoryReservationFailed.OrderId, inventoryReservationFailed.Reason);
            await _orderService.UpdateOrderStatusAsync(inventoryReservationFailed.OrderId,OrderStatus.Cancelled.ToString());
        }

        public async Task HandleInventoryReservedAsync(InventoryReserved inventoryReserved)
        {
            _logger.LogInformation("Inventory reserved for OrderId:{OrderId}", inventoryReserved.OrderId);
            await _orderService.UpdateOrderStatusAsync(inventoryReserved.OrderId, OrderStatus.InventoryReserved.ToString());
        }

        public async Task HandlePaymentFailedAsync(PaymentFailed paymentFailed)
        {
            _logger.LogInformation("Payment failed for OrderId:{OrderId} Reason:{Reason}", paymentFailed.OrderId,paymentFailed.Reason);
            await _orderService.UpdateOrderStatusAsync(paymentFailed.OrderId,OrderStatus.Cancelled.ToString());
        }

        public async Task HandlePaymentProcessedAsync(PaymentProcessed paymentProcessed)
        {
            _logger.LogInformation("Payment processed for OrderId:{OrderId}", paymentProcessed.OrderId);
            await _orderService.UpdateOrderStatusAsync(paymentProcessed.OrderId, OrderStatus.Confirmed.ToString());
        }
    }
}
