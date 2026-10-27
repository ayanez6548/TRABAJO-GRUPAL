using NorthWind.Result.Entities;
using NorthWind.Membership.Entities.Dtos;
namespace NorthWind.Membership.Backend.Core.Interfaces;

public interface IUserRegistrationInputPort
{
    Task Handle(UserRegistrationDto dto, CancellationToken cancellationToken = default);
}
public interface IUserRegistrationOutputPort
{
    Result<string>? Result { get; }
    Task Handle(Result<string> result, CancellationToken cancellationToken = default);
}
public interface IUserLoginInputPort
{
    Task Handle(UserCredentialsDto dto, CancellationToken cancellationToken = default);
}
public interface IUserLoginOutputPort
{
    Result<TokensDto>? Result { get; }
    Task Handle(Result<UserDto> result, CancellationToken cancellationToken = default);
}
