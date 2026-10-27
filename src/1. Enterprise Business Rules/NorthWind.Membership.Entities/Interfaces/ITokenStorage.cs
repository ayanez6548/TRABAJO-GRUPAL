using NorthWind.Membership.Entities.Dtos;
namespace NorthWind.Membership.Entities.Interfaces;
public interface ITokenStorage
{
    Task<TokensDto?> GetTokensAsync();
    Task StoreTokensAsync(TokensDto tokens);
    Task RemoveTokensAsync();
}
