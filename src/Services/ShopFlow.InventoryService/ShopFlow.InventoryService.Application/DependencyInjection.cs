using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using ShopFlow.InventoryService.Application.Interfaces;
using ShopFlow.InventoryService.Application.Services;
using ShopFlow.InventoryService.Application.Validators;

namespace ShopFlow.InventoryService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IInventoryService,Services.InventoryService>();

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator> ();
        services.AddScoped<IInventoryEventHandler, InventoryEventHandler>();

        return services;
    }
}