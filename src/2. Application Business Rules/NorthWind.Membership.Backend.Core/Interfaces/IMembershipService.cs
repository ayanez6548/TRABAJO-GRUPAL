using NorthWind.Result.Entities;
using NorthWind.Membership.Entities.Dtos;
namespace NorthWind.Membership.Backend.Core.Interfaces;
public interface IMembershipService
{
    Task<Result<string>> Register(UserRegistrationDto dto, CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByCredentials(UserCredentialsDto dto, CancellationToken cancellationToken = default);
}
public interface ITokenService { TokensDto CreateToken(UserDto user); }
