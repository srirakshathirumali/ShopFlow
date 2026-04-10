using FluentValidation;
using ShopFlow.InventoryService.Application.DTOs;

namespace ShopFlow.InventoryService.Application.Validators;

public class CreateProductRequestValidator
    : AbstractValidator<CreateProductRequestDto>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters.");

        RuleFor(x => x.InitialStock)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Initial stock cannot be negative.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");
    }
}