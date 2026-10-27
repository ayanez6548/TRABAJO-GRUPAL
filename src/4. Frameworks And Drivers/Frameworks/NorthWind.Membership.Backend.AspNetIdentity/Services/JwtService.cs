using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NorthWind.Membership.Backend.Core.Interfaces;
using NorthWind.Membership.Entities.Dtos;
namespace NorthWind.Membership.Backend.AspNetIdentity.Services;

public sealed class JwtOptions
{
    public string SecurityKey { get; set; } = "";
    public string ValidIssuer { get; set; } = "NorthWind";
    public string ValidAudience { get; set; } = "NorthWind.Client";
    public int ExpireInMinutes { get; set; } = 30;
}
public sealed class JwtService(IOptions<JwtOptions> options) : ITokenService
{
    public TokensDto CreateToken(UserDto user)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(options.Value.ExpireInMinutes);
        var token = new JwtSecurityToken(options.Value.ValidIssuer, options.Value.ValidAudience,
            [new(JwtRegisteredClaimNames.Sub, user.Id), new(JwtRegisteredClaimNames.Email, user.Email),
             new("name", user.Email), new("FullName", $"{user.FirstName} {user.LastName}"),
             new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())],
            now, expires, new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.SecurityKey)), SecurityAlgorithms.HmacSha256));
        return new(new JwtSecurityTokenHandler().WriteToken(token), new DateTimeOffset(expires, TimeSpan.Zero));
    }
}
