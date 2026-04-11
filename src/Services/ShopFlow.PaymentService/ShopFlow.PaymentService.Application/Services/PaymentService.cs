using ShopFlow.PaymentService.Application.DTOs;
using ShopFlow.PaymentService.Application.Interfaces;
using ShopFlow.PaymentService.Domain.Entities;
using ShopFlow.PaymentService.Domain.Enums;
using ShopFlow.PaymentService.Domain.Exceptions;
using ShopFlow.PaymentService.Domain.Interfaces;

namespace ShopFlow.PaymentService.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentResponseDto> ProcessPaymentAsync(ProcessPaymentRequestDto request)
    {
        var existing = await _paymentRepository
            .GetByOrderIdAsync(request.OrderId);

        if (existing is not null)
            throw new PaymentAlreadyProcessedException(request.OrderId);

        // Simulate payment processing
        // In production this calls a real payment gateway
        var isSuccess = SimulatePaymentGateway(request.Amount);

        var payment = new Payment
        {
            OrderId = request.OrderId,
            Amount = request.Amount,
            Status = isSuccess
                ? PaymentStatus.Processed
                : PaymentStatus.Failed,
            FailureReason = isSuccess
                ? null
                : "Payment declined by gateway.",
            ProcessedAt = DateTime.UtcNow
        };

        await _paymentRepository.AddAsync(payment);
        return MapToDto(payment);
    }

    public async Task<PaymentResponseDto> GetPaymentByOrderIdAsync(Guid orderId)
    {
        var payment = await _paymentRepository.GetByOrderIdAsync(orderId)
            ?? throw new PaymentNotFoundException(orderId);

        return MapToDto(payment);
    }

    // Simulates a payment gateway — succeeds 90% of the time
    private static bool SimulatePaymentGateway(decimal amount) =>
        amount > 0 && new Random().Next(1, 11) > 1;

    private static PaymentResponseDto MapToDto(Payment payment) =>
        new()
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Status = payment.Status,
            FailureReason = payment.FailureReason,
            ProcessedAt = payment.ProcessedAt,
            CreatedAt = payment.CreatedAt
        };
}