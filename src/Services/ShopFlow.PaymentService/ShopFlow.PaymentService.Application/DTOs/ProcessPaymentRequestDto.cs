namespace ShopFlow.PaymentService.Application.DTOs;

public class ProcessPaymentRequestDto
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
}