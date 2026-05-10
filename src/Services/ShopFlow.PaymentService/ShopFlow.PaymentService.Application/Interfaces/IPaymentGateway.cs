namespace ShopFlow.PaymentService.Application.Interfaces;

public class PaymentGatewayRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
}

public class PaymentGatewayResponse
{
    public bool IsSuccess { get; set; }
    public string? FailureReason { get; set; }
    public string TransactionId { get; set; } = string.Empty;
}

public interface IPaymentGateway
{
    Task<PaymentGatewayResponse> ProcessAsync(PaymentGatewayRequest request);
}