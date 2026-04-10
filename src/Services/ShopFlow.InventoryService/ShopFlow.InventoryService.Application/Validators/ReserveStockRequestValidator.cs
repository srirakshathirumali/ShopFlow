using FluentValidation;
using ShopFlow.InventoryService.Application.DTOs;

namespace ShopFlow.InventoryService.Application.Validators;

public class ReserveStockRequestValidator
    : AbstractValidator<ReserveStockRequestDto>
{
    public ReserveStockRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Must reserve at least one item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required.");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be at least 1.");
        });
    }
}