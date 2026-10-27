using System.Text.Json;
using Microsoft.JSInterop;
using NorthWind.Membership.Entities.Dtos;
using NorthWind.Membership.Entities.Interfaces;
namespace NorthWind.Membership.Frontend.RazorViews.Services;
public sealed class TokenLocalStorage(IJSRuntime js) : ITokenStorage
{
    private const string Key = "northwind.tokens";
    public async Task<TokensDto?> GetTokensAsync()
    {
        var value = await js.InvokeAsync<string?>("localStorage.getItem", Key);
        if (string.IsNullOrEmpty(value)) return null;
        try
        {
            var tokens = JsonSerializer.Deserialize<TokensDto>(value);
            if (tokens is not null && tokens.ExpiresAt > DateTimeOffset.UtcNow) return tokens;
        }
        catch (JsonException) { }
        await RemoveTokensAsync();
        return null;
    }
    public Task StoreTokensAsync(TokensDto tokens) => js.InvokeVoidAsync("localStorage.setItem", Key, JsonSerializer.Serialize(tokens)).AsTask();
    public Task RemoveTokensAsync() => js.InvokeVoidAsync("localStorage.removeItem", Key).AsTask();
}
