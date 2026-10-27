using NorthWind.Result.Entities;
using NorthWind.Validation.Entities;
using NorthWind.Membership.Backend.Core.Interfaces;
using NorthWind.Membership.Entities.Dtos;
namespace NorthWind.Membership.Backend.Core.UseCases;

public sealed class UserRegistrationInteractor(IMembershipService membership, IUserRegistrationOutputPort presenter,
    IModelValidatorHub<UserRegistrationDto> validator) : IUserRegistrationInputPort
{
    public async Task Handle(UserRegistrationDto dto, CancellationToken cancellationToken = default)
    {
        var errors = await validator.Validate(dto, cancellationToken);
        var result = errors.Count > 0 ? Result<string>.Failure(errors) : await membership.Register(dto, cancellationToken);
        await presenter.Handle(result, cancellationToken);
    }
}
public sealed class UserLoginInteractor(IMembershipService membership, IUserLoginOutputPort presenter,
    IModelValidatorHub<UserCredentialsDto> validator) : IUserLoginInputPort
{
    public async Task Handle(UserCredentialsDto dto, CancellationToken cancellationToken = default)
    {
        var errors = await validator.Validate(dto, cancellationToken);
        if (errors.Count > 0)
        {
            await presenter.Handle(Result<UserDto>.Failure(errors), cancellationToken);
            return;
        }
        var user = await membership.GetUserByCredentials(dto, cancellationToken);
        var result = user is null ? Result<UserDto>.Failure(new ValidationError("Credentials", "El correo electrónico o la contraseña son incorrectos."))
            : Result<UserDto>.Success(user);
        await presenter.Handle(result, cancellationToken);
    }
}
