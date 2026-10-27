using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using NorthWind.Membership.Entities.Dtos;
using NorthWind.Membership.Entities.Interfaces;
namespace NorthWind.Membership.Frontend.RazorViews.AuthenticationStateProviders;

public sealed class JWTAuthenticationStateProvider(ITokenStorage storage) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var tokens = await storage.GetTokensAsync();
        if (tokens is not null)
        {
            try
            {
                string payload = tokens.AccessToken.Split('.')[1].Replace('-', '+').Replace('_', '/');
                payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                using var json = JsonDocument.Parse(Convert.FromBase64String(payload));
                if (json.RootElement.GetProperty("exp").GetInt64() > DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    var claims = json.RootElement.EnumerateObject().Select(p => new Claim(p.Name, p.Value.ToString()));
                    return new(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt", "name", "role")));
                }
            }
            catch (Exception ex) when (ex is JsonException or FormatException or IndexOutOfRangeException or KeyNotFoundException or InvalidOperationException) { }
            await storage.RemoveTokensAsync();
        }
        return new(new ClaimsPrincipal(new ClaimsIdentity()));
    }
    public async Task LoginAsync(TokensDto tokens)
    {
        await storage.StoreTokensAsync(tokens);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
    public async Task LogoutAsync()
    {
        await storage.RemoveTokensAsync();
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
