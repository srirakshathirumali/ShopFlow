using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using ShopFlow.PaymentService.Application.Interfaces;
using ShopFlow.PaymentService.Application.Validators;

namespace ShopFlow.PaymentService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IPaymentService,
            Services.PaymentService>();

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<ProcessPaymentRequestValidator> ();

        return services;
    }
}