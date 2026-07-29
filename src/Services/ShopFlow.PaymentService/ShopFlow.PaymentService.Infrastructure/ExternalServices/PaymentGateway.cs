using Microsoft.Extensions.Logging;
using ShopFlow.PaymentService.Application.Interfaces;

namespace ShopFlow.PaymentService.Infrastructure.ExternalServices;

public class PaymentGateway : IPaymentGateway
{
    private readonly ILogger<PaymentGateway> _logger;
    private static int _callCount = 0;

    public PaymentGateway(ILogger<PaymentGateway> logger)
    {
        _logger = logger;
    }

    public async Task<PaymentGatewayResponse> ProcessAsync(PaymentGatewayRequest request)
    {
        _callCount++;

        _logger.LogInformation(
            "PaymentGateway.ProcessAsync called — OrderId: {OrderId} " +
            "Amount: {Amount} CallCount: {Count}",
            request.OrderId, request.Amount, _callCount);

        await Task.Delay(TimeSpan.FromMilliseconds(200));

        var isSuccess = new Random().Next(1, 11) <= 8;

        if (!isSuccess)
            throw new HttpRequestException("Payment gateway unavailable.");

        return new PaymentGatewayResponse
        {
            IsSuccess = true,
            TransactionId = Guid.NewGuid().ToString(),
            FailureReason = null
        };
    }
}