using FluentValidation;
using ShopFlow.PaymentService.Application.DTOs;

namespace ShopFlow.PaymentService.Application.Validators;

public class ProcessPaymentRequestValidator
    : AbstractValidator<ProcessPaymentRequestDto>
{
    public ProcessPaymentRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than 0.");
    }
}