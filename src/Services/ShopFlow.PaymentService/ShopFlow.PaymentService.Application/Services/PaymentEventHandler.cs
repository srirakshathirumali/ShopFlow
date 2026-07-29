using Microsoft.Extensions.Logging;
using ShopFlow.Contracts.Events;
using ShopFlow.PaymentService.Application.Interfaces;
using ShopFlow.PaymentService.Domain.Entities;
using ShopFlow.PaymentService.Domain.Enums;
using ShopFlow.PaymentService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShopFlow.PaymentService.Application.Services
{
    public class PaymentEventHandler : IPaymentEventHandler
    {
        private readonly ILogger<PaymentEventHandler> _logger;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IPaymentGateway _paymentGateway;

        public PaymentEventHandler(ILogger<PaymentEventHandler> logger, IPaymentRepository paymentRepository, IEventPublisher eventPublisher, IPaymentGateway paymentGateway)
        {
            _logger = logger;
            _eventPublisher = eventPublisher;
            _paymentRepository = paymentRepository;
            _paymentGateway = paymentGateway;
        }
        public async Task HandleInventoryReservedAsync(InventoryReserved inventoryReserved)
        {


            _logger.LogInformation("InventoryReserved for OrderId:{OrderId}", inventoryReserved.OrderId);

            var existing = await _paymentRepository.GetByOrderIdAsync(inventoryReserved.OrderId);

            if (existing is not null)
            {
                _logger.LogInformation("Payment already processed for OrderId: {OrderId}", inventoryReserved.OrderId);
                return;
            }
            PaymentGatewayResponse gatewayResponse;

                // Call payment gateway through Circuit Breaker
                gatewayResponse = await _paymentGateway.ProcessAsync(
                    new PaymentGatewayRequest
                    {
                        OrderId = inventoryReserved.OrderId,
                        Amount = inventoryReserved.TotalAmount
                    });

            var payment = new Payment
            {
                OrderId = inventoryReserved.OrderId,
                Amount = inventoryReserved.TotalAmount,
                Status = gatewayResponse.IsSuccess
                               ? PaymentStatus.Processed
                               : PaymentStatus.Failed,
                FailureReason = gatewayResponse.FailureReason,
                ProcessedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment);

            if (gatewayResponse.IsSuccess)
            {
                _logger.LogInformation("Payment successful for OrderId: {OrderId}", inventoryReserved.OrderId);

                await _eventPublisher.PublishAsync(new PaymentProcessed
                {
                    OrderId = inventoryReserved.OrderId,
                    PaymentId = payment.Id,
                    Amount = payment.Amount,
                    ProcessedAt = DateTime.UtcNow
                });
            }
            else
            {
                _logger.LogInformation("Payment failed for OrderId: {OrderId} Reason: {Reason}", inventoryReserved.OrderId, gatewayResponse.FailureReason);

                await _eventPublisher.PublishAsync(new PaymentFailed
                {
                    OrderId = inventoryReserved.OrderId,
                    Reason = gatewayResponse.FailureReason ?? "Payment Failed",
                    FailedAt = DateTime.UtcNow
                });
            }
        }
    }
}
