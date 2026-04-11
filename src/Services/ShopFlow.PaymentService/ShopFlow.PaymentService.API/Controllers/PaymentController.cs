using Microsoft.AspNetCore.Mvc;
using ShopFlow.PaymentService.Application.DTOs;
using ShopFlow.PaymentService.Application.Interfaces;

namespace ShopFlow.PaymentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>Processes payment for an order.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponseDto),StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequestDto request)
    {
        var result = await _paymentService.ProcessPaymentAsync(request);
        return CreatedAtAction(
            nameof(GetPaymentByOrder),
            new { orderId = result.OrderId }, result);
    }

    /// <summary>Gets payment details for an order.</summary>
    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(PaymentResponseDto),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentByOrder(Guid orderId)
    {
        var result = await _paymentService
            .GetPaymentByOrderIdAsync(orderId);
        return Ok(result);
    }
}