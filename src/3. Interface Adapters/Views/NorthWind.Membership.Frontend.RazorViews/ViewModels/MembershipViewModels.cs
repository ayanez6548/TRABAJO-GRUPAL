using NorthWind.Exceptions.Entities;
using NorthWind.Validation.Entities;
using NorthWind.Membership.Entities.Dtos;
using NorthWind.Membership.Frontend.RazorViews.AuthenticationStateProviders;
using NorthWind.Sales.Frontend.WebApiGateways.WebApiGateways;
namespace NorthWind.Membership.Frontend.RazorViews.ViewModels;
public sealed class UserRegistrationViewModel(MembershipGateway gateway, IModelValidatorHub<UserRegistrationDto> validator)
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
    public async Task Register()
    {
        var dto = new UserRegistrationDto(Email, Password, FirstName, LastName);
        var errors = (await validator.Validate(dto)).ToList();
        if (Password != ConfirmPassword) errors.Add(new("ConfirmPassword", "Las contraseñas no coinciden."));
        if (errors.Count > 0) throw new ValidationException(errors);
        await gateway.Register(dto);
        Password = ConfirmPassword = "";
    }
}
public sealed class UserLoginViewModel(MembershipGateway gateway, IModelValidatorHub<UserCredentialsDto> validator,
    JWTAuthenticationStateProvider authentication)
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public async Task Login()
    {
        var dto = new UserCredentialsDto(Email, Password);
        var errors = await validator.Validate(dto);
        if (errors.Count > 0) throw new ValidationException(errors);
        await authentication.LoginAsync(await gateway.Login(dto));
        Password = "";
    }
}
