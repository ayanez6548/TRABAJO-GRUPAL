using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NorthWind.HttpDelegatingHandlers;
using NorthWind.Membership.Entities.Dtos;
using NorthWind.Membership.Entities.Interfaces;
using NorthWind.Membership.Frontend.RazorViews.AuthenticationStateProviders;
using NorthWind.Membership.Frontend.RazorViews.Services;
using NorthWind.Membership.Frontend.RazorViews.ViewModels;
using NorthWind.Sales.Entities.Dtos.CreateOrder;
using NorthWind.Sales.Frontend.BusinessObjects.Interfaces;
using NorthWind.Sales.Frontend.BusinessObjects.ViewModels;
using NorthWind.Sales.Frontend.WebApiGateways.WebApiGateways;
using NorthWind.Sales.Validators.Entities;
using NorthWind.Validation.Entities;
using NorthWind.ValidationService.FluentValidation;

namespace NorthWind.Sales.Frontend.IoC;

public static class DependencyContainer
{
    public static IServiceCollection AddNorthWindFrontendServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorizationCore();
        services.AddScoped<ITokenStorage, TokenLocalStorage>();
        services.AddScoped<JWTAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(p => p.GetRequiredService<JWTAuthenticationStateProvider>());
        services.AddScoped(p => new HttpClient(new MembershipBearerTokenHandler(p.GetRequiredService<ITokenStorage>())
        {
            InnerHandler = new ExceptionDelegatingHandler { InnerHandler = new HttpClientHandler() }
        }) { BaseAddress = new Uri(configuration["ApiBaseUrl"] ?? "http://localhost:5100") });
        services.AddScoped<ICreateOrderGateway, CreateOrderGateway>();
        services.AddScoped<MembershipGateway>();
        services.AddTransient<CreateOrderViewModel>();
        services.AddTransient<UserRegistrationViewModel>();
        services.AddTransient<UserLoginViewModel>();
        services.AddScoped(typeof(IModelValidatorHub<>), typeof(ModelValidatorHub<>));
        services.AddScoped<IModelValidator<CreateOrderDto>, CreateOrderDtoValidator>();
        services.AddScoped<IModelValidator<UserRegistrationDto>, UserRegistrationDtoValidator>();
        services.AddScoped<IModelValidator<UserCredentialsDto>, UserCredentialsDtoValidator>();
        return services;
    }
}
