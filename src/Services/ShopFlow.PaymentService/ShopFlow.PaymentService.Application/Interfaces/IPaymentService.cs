using ShopFlow.PaymentService.Application.DTOs;

namespace ShopFlow.PaymentService.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponseDto> ProcessPaymentAsync(
        ProcessPaymentRequestDto request);
    Task<PaymentResponseDto> GetPaymentByOrderIdAsync(Guid orderId);
}