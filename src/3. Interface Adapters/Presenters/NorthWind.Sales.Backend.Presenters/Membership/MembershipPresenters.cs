using NorthWind.Result.Entities;
using NorthWind.Membership.Backend.Core.Interfaces;
using NorthWind.Membership.Entities.Dtos;
namespace NorthWind.Sales.Backend.Presenters.Membership;

public sealed class UserRegistrationPresenter : IUserRegistrationOutputPort
{
    public Result<string>? Result { get; private set; }
    public Task Handle(Result<string> result, CancellationToken cancellationToken = default)
    {
        Result = result;
        return Task.CompletedTask;
    }
}
public sealed class UserLoginPresenter(ITokenService tokens) : IUserLoginOutputPort
{
    public Result<TokensDto>? Result { get; private set; }
    public Task Handle(Result<UserDto> result, CancellationToken cancellationToken = default)
    {
        Result = result.HasError ? Result<TokensDto>.Failure(result.Errors)
            : Result<TokensDto>.Success(tokens.CreateToken(result.Value!));
        return Task.CompletedTask;
    }
}
