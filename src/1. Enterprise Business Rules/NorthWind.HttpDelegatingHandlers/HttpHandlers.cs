using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using NorthWind.Exceptions.Entities;
using NorthWind.Validation.Entities;
using NorthWind.Membership.Entities.Interfaces;
namespace NorthWind.HttpDelegatingHandlers;

public sealed class MembershipBearerTokenHandler(ITokenStorage storage) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await storage.GetTokensAsync();
        if (token is not null && token.ExpiresAt > DateTimeOffset.UtcNow)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        return await base.SendAsync(request, cancellationToken);
    }
}
public sealed class ExceptionDelegatingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try { response = await base.SendAsync(request, cancellationToken); }
        catch (HttpRequestException ex) { throw new HttpRequestException("No se pudo conectar con el servicio. Revisa tu conexión e inténtalo nuevamente.", ex); }
        if (response.IsSuccessStatusCode) return response;
        using (response)
        {
            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedAccessException("Tu sesión venció o las credenciales son incorrectas. Inicia sesión nuevamente.");
            try
            {
                using var json = JsonDocument.Parse(body);
                if (json.RootElement.TryGetProperty("errors", out var errors))
                    throw new ValidationException(errors.EnumerateObject().SelectMany(p => p.Value.EnumerateArray()
                        .Select(e => new ValidationError(p.Name, e.GetString() ?? "Valor no válido."))).ToArray());
                var title = json.RootElement.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String
                    ? detail.GetString() : json.RootElement.TryGetProperty("message", out var message) ? message.GetString()
                    : json.RootElement.TryGetProperty("title", out var t) ? t.GetString() : null;
                throw new HttpRequestException(title ?? "No se pudo completar la solicitud.", null, response.StatusCode);
            }
            catch (JsonException) { throw new HttpRequestException("No se pudo completar la operación. Verifica que el servicio esté disponible.", null, response.StatusCode); }
        }
    }
}
