using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using ShopFlow.PaymentService.Application.Interfaces;
using ShopFlow.PaymentService.Infrastructure.ExternalServices;

namespace ShopFlow.PaymentService.Infrastructure.Resilience;

public class ResilientPaymentGateway : IPaymentGateway
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ResilientPaymentGateway> _logger;
    private readonly AsyncCircuitBreakerPolicy _circuitBreaker;

    public ResilientPaymentGateway(
        IServiceProvider serviceProvider,
        ILogger<ResilientPaymentGateway> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        _circuitBreaker = Policy
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    _logger.LogWarning(
                        "*** Circuit OPEN — Payment gateway unavailable. " +
                        "Breaking for {Duration}s. Reason: {Message}",
                        duration.TotalSeconds,
                        exception.Message);
                },
                onReset: () =>
                {
                    _logger.LogInformation(
                        "*** Circuit CLOSED — Payment gateway recovered.");
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation(
                        "*** Circuit HALF-OPEN — Testing payment gateway...");
                });
    }

    public async Task<PaymentGatewayResponse> ProcessAsync(
        PaymentGatewayRequest request)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var inner = scope.ServiceProvider
                .GetRequiredService<PaymentGateway>();

            // HttpRequestException must propagate OUT of this lambda
            // to the circuit breaker — no try/catch inside the lambda
            return await _circuitBreaker.ExecuteAsync(async () =>
            {
                var result = await inner.ProcessAsync(request);
                return result;
            });
        }
        catch (BrokenCircuitException)
        {
            _logger.LogWarning(
                "*** Circuit is OPEN — rejecting payment immediately " +
                "for OrderId: {OrderId}", request.OrderId);

            return new PaymentGatewayResponse
            {
                IsSuccess = false,
                FailureReason = "Payment service temporarily unavailable. " +
                                "Please try again in 30 seconds.",
                TransactionId = string.Empty
            };
        }
        catch (HttpRequestException ex)
        {
            // This catches failures AFTER circuit breaker counts them
            _logger.LogError(
                "Payment gateway failed for OrderId: {OrderId} — {Message}",
                request.OrderId, ex.Message);

            return new PaymentGatewayResponse
            {
                IsSuccess = false,
                FailureReason = "Payment gateway error. Please try again.",
                TransactionId = string.Empty
            };
        }
    }
}