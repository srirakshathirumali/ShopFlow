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
        public PaymentEventHandler(ILogger<PaymentEventHandler> logger, IPaymentRepository paymentRepository, IEventPublisher eventPublisher)
        {
            _logger = logger;
            _eventPublisher = eventPublisher;
            _paymentRepository = paymentRepository;
        }
        public async Task HandleInventoryReservedAsync(InventoryReserved inventoryReserved)
        {
            _logger.LogInformation("InventoryReserved for OrderId:{OrderId}", inventoryReserved.OrderId);

            var existing = await _paymentRepository.GetByOrderIdAsync(inventoryReserved.OrderId);

            if(existing is not null)
            {
                _logger.LogInformation("Payment already processed for Order Id:{0}", inventoryReserved.OrderId);
                return;
            }

            try
            {
                var paymentResult = await SimulatePaymentGatewayAsync(inventoryReserved.OrderId);

                var payment = new Payment
                {
                    OrderId = inventoryReserved.OrderId,
                    Amount = paymentResult.Amount,
                    Status = paymentResult.IsSuccess
                                  ? PaymentStatus.Processed
                                  : PaymentStatus.Failed,
                    FailureReason = paymentResult.IsSuccess
                                  ? null
                                  : paymentResult.FailureReason,
                    ProcessedAt = DateTime.UtcNow
                };

                await _paymentRepository.AddAsync(payment);

                if(paymentResult.IsSuccess)
                {
                    _logger.LogInformation("Payment successfull for Order Id:{0}", inventoryReserved.OrderId);

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
                    _logger.LogInformation("Payment failed for Order Id:{0} Reason:{1}", inventoryReserved.OrderId,paymentResult.FailureReason);

                    await _eventPublisher.PublishAsync(new PaymentFailed
                    {
                        OrderId = inventoryReserved.OrderId,
                        Reason = paymentResult.FailureReason,
                        FailedAt = DateTime.UtcNow
                    });
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occured while processing payment for Order Id{0}", inventoryReserved.OrderId);
                await _eventPublisher.PublishAsync(new PaymentFailed
                {
                    OrderId = inventoryReserved.OrderId,
                    Reason = "Unexpected payment processing error.",
                    FailedAt = DateTime.UtcNow
                });
            }
        }


        private static Task<PaymentGatewayResult> SimulatePaymentGatewayAsync( Guid orderId)
        {
            // Simulate 80% success rate
            // In production replace with real gateway call
            var random = new Random();
            var isSuccess = random.Next(1, 11) <= 8;
           // var isSuccess = false;

            var result = new PaymentGatewayResult
            {
                IsSuccess = isSuccess,
                Amount = random.Next(100, 5000),
                FailureReason = isSuccess
                                    ? null
                                    : "Card declined by issuing bank."
            };

            return Task.FromResult(result);
        }
    }

    public class PaymentGatewayResult
    {
        public bool IsSuccess { get; set; }
        public decimal Amount { get; set; }
        public string? FailureReason { get; set; }
    }

}


