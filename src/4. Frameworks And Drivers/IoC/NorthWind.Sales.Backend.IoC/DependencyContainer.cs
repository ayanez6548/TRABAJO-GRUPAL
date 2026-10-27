using NorthWind.Sales.Validators.Entities;
using NorthWind.DomainLogs.Entities;
using NorthWind.Transactions.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NorthWind.Events.Entities;
using NorthWind.Entities.Interfaces;
using NorthWind.Validation.Entities;
using NorthWind.Membership.Backend.AspNetIdentity.Entities;
using NorthWind.Membership.Backend.AspNetIdentity.Services;
using NorthWind.Membership.Backend.Core.Interfaces;
using NorthWind.Membership.Backend.Core.UseCases;
using NorthWind.Membership.Entities.Dtos;
using NorthWind.Sales.Backend.BusinessObjects.Interfaces;
using NorthWind.Sales.Backend.DataContexts.EFCore.DataContexts;
using NorthWind.Sales.Backend.DataContexts.EFCore.Services;
using NorthWind.Sales.Backend.SmtpGateways;
using NorthWind.UserServices;
using NorthWind.Sales.Backend.Presenters.CreateOrder;
using NorthWind.Sales.Backend.Presenters.Membership;
using NorthWind.Sales.Backend.Repositories.Interfaces;
using NorthWind.Sales.Backend.Repositories.Repositories;
using NorthWind.Sales.Backend.UseCases;
using NorthWind.Sales.Backend.UseCases.Common;
using NorthWind.Sales.Entities.Dtos.CreateOrder;
using NorthWind.ValidationService.FluentValidation;
namespace NorthWind.Sales.Backend.IoC;

public static class DependencyContainer
{
    public static IServiceCollection AddNorthWindSalesServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("NorthWind")
            ?? throw new InvalidOperationException("ConnectionStrings:NorthWind is required.");
        var provider = configuration["Database:Provider"] ?? "SqlServer";
        if (provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<PostgreSqlNorthWindContext>(o => o.UseNpgsql(connection));
            services.AddScoped<NorthWindContext>(p => p.GetRequiredService<PostgreSqlNorthWindContext>());
        }
        else if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            if (!OperatingSystem.IsWindows() && connection.Contains("(localdb)", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("LocalDB is Windows-only. Configure Database:Provider and ConnectionStrings:NorthWind for Debian.");
            services.AddDbContext<NorthWindContext>(o => o.UseSqlServer(connection));
        }
        else throw new InvalidOperationException("Database:Provider must be SqlServer or PostgreSql.");
        services.AddScoped<INorthWindSalesCommandsDataContext>(p => p.GetRequiredService<NorthWindContext>());
        services.AddScoped<INorthWindSalesQueriesDataContext>(p => p.GetRequiredService<NorthWindContext>());
        services.AddScoped<INorthWindDomainLogsDataContext>(p => p.GetRequiredService<NorthWindContext>());
        services.AddScoped<ICommandsRepository, CommandsRepository>();
        services.AddScoped<IQueriesRepository, QueriesRepository>();
        services.AddScoped<ICreateOrderOutputPort, CreateOrderPresenter>();
        services.AddScoped<IDomainLogger, DomainLogger>();
        services.AddScoped<IDomainLogsRepository, DomainLogsRepository>();
        services.AddScoped<IDomainTransaction, DomainTransaction>();
        services.AddScoped(typeof(IDomainEventHub<>), typeof(DomainEventHub<>));
        services.AddScoped(typeof(IModelValidatorHub<>), typeof(ModelValidatorHub<>));
        services.AddScoped<IModelValidator<CreateOrderDto>, CreateOrderDtoValidator>();
        services.AddScoped<IModelValidator<UserRegistrationDto>, UserRegistrationDtoValidator>();
        services.AddScoped<IModelValidator<UserCredentialsDto>, UserCredentialsDtoValidator>();
        services.AddUseCasesServices();
        services.AddHttpContextAccessor();
        services.AddScoped<IUserService, UserService>();
        services.AddTransient<IMailService, MailService>();
        services.Configure<SmtpOptions>(configuration.GetSection("SmtpOptions"));
        services.AddIdentityCore<NorthWindUser>(o =>
        {
            o.User.RequireUniqueEmail = true;
            o.User.AllowedUserNameCharacters = MembershipEmailPolicy.AllowedCharacters;
            o.Password.RequiredLength = 6;
            o.Password.RequireLowercase = o.Password.RequireUppercase = o.Password.RequireDigit = o.Password.RequireNonAlphanumeric = true;
            o.Lockout.MaxFailedAccessAttempts = 5;
            o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        }).AddEntityFrameworkStores<NorthWindContext>().AddSignInManager();
        services.AddScoped<IMembershipService, MembershipService>();
        services.AddScoped<ITokenService, JwtService>();
        services.AddScoped<IUserRegistrationInputPort, UserRegistrationInteractor>();
        services.AddScoped<IUserRegistrationOutputPort, UserRegistrationPresenter>();
        services.AddScoped<IUserLoginInputPort, UserLoginInteractor>();
        services.AddScoped<IUserLoginOutputPort, UserLoginPresenter>();
        return services;
    }
}
