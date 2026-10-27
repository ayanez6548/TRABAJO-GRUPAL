using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthWind.Validation.Entities;
using NorthWind.Membership.Backend.AspNetIdentity.Services;
using NorthWind.Membership.Entities.Dtos;
using NorthWind.ValidationService.FluentValidation;
namespace NorthWind.Tests;
[TestClass]
public sealed class MembershipTests
{
    [TestMethod]
    public async Task Registration_EnforcesPasswordAndEmailRules()
    {
        IModelValidator<UserRegistrationDto> validator = new UserRegistrationDtoValidator();
        Assert.IsEmpty(await validator.Validate(new("ana@example.com", "Clave123!", "Ana", "Pérez")));
        Assert.IsGreaterThanOrEqualTo(3, (await validator.Validate(new("bad", "ABCDEF", "", ""))).Count);
    }
    [TestMethod]
    public void Jwt_ContainsUserAndPassesSignatureIssuerAudienceAndLifetimeValidation()
    {
        var options = new JwtOptions { SecurityKey = new string('K', 64), ValidIssuer = "test", ValidAudience = "client", ExpireInMinutes = 5 };
        var tokens = new JwtService(Options.Create(options)).CreateToken(new("user1", "ana@example.com", "Ana", "Pérez"));
        var parameters = new TokenValidationParameters {
            ValidateIssuer = true, ValidIssuer = options.ValidIssuer, ValidateAudience = true, ValidAudience = options.ValidAudience,
            ValidateLifetime = true, ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecurityKey)), NameClaimType = "name"
        };
        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var principal = handler.ValidateToken(tokens.AccessToken, parameters, out _);
        Assert.AreEqual("ana@example.com", principal.Identity!.Name);
        Assert.AreEqual("Ana Pérez", principal.FindFirst("FullName")?.Value);
        parameters.ValidAudience = "wrong";
        Assert.ThrowsExactly<SecurityTokenInvalidAudienceException>(() => handler.ValidateToken(tokens.AccessToken, parameters, out _));
        parameters.ValidAudience = options.ValidAudience;
        parameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(new string('Z', 64)));
        Assert.Throws<SecurityTokenException>(() => handler.ValidateToken(tokens.AccessToken, parameters, out _));
    }
    [TestMethod]
    public void Practice16_RejectsExpiredTokenEvenWithCorrectSignature()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(new string('K', 64)));
        var expired = new JwtSecurityToken("issuer", "audience", [], DateTime.UtcNow.AddMinutes(-2),
            DateTime.UtcNow.AddMinutes(-1), new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        var handler = new JwtSecurityTokenHandler();
        var parameters = new TokenValidationParameters {
            ValidateIssuer = true, ValidIssuer = "issuer", ValidateAudience = true, ValidAudience = "audience",
            ValidateLifetime = true, ValidateIssuerSigningKey = true, IssuerSigningKey = key, ClockSkew = TimeSpan.Zero
        };
        Assert.ThrowsExactly<SecurityTokenExpiredException>(() => handler.ValidateToken(handler.WriteToken(expired), parameters, out _));
    }
}
