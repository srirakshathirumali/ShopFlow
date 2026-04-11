using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using ShopFlow.OrderService.Application.Interfaces;
using ShopFlow.OrderService.Application.Validators;

namespace ShopFlow.OrderService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IOrderService,Services.OrderService>();

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

        return services;
    }
}