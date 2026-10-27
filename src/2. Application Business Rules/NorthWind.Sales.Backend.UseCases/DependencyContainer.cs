using Microsoft.Extensions.DependencyInjection;
using NorthWind.Events.Entities;
using NorthWind.Validation.Entities;
using NorthWind.Sales.Backend.BusinessObjects.Events;
using NorthWind.Sales.Backend.BusinessObjects.Interfaces;
using NorthWind.Sales.Backend.UseCases.CreateOrder;
using NorthWind.Sales.Entities.Dtos.CreateOrder;
namespace NorthWind.Sales.Backend.UseCases;
public static class DependencyContainer
{
    public static IServiceCollection AddUseCasesServices(this IServiceCollection services) => services
        .AddScoped<ICreateOrderInputPort, CreateOrderInteractor>()
        .AddScoped<IModelValidator<CreateOrderDto>, CreateOrderCustomerValidator>()
        .AddScoped<IModelValidator<CreateOrderDto>, CreateOrderProductValidator>()
        .AddScoped<IDomainEventHandler<SpecialOrderCreatedEvent>, SendEMailWhenSpecialOrderCreatedEventHandler>();
}
