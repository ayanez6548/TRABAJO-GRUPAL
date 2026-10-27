using Microsoft.AspNetCore.Identity;
using NorthWind.Result.Entities;
using NorthWind.Validation.Entities;
using NorthWind.Membership.Backend.AspNetIdentity.Entities;
using NorthWind.Membership.Backend.Core.Interfaces;
using NorthWind.Membership.Entities.Dtos;
namespace NorthWind.Membership.Backend.AspNetIdentity.Services;

public sealed class MembershipService(UserManager<NorthWindUser> manager, SignInManager<NorthWindUser> signIn) : IMembershipService
{
    public async Task<Result<string>> Register(UserRegistrationDto dto, CancellationToken cancellationToken = default)
    {
        var user = new NorthWindUser { UserName = dto.Email, Email = dto.Email,
            FirstName = dto.FirstName.Trim(), LastName = dto.LastName.Trim() };
        var result = await manager.CreateAsync(user, dto.Password);
        return result.Succeeded ? Result<string>.Success(user.Id)
            : Result<string>.Failure(result.Errors.Select(e => new ValidationError(
                e.Code.Contains("Email") || e.Code.Contains("UserName") ? "Email" : "Password", Translate(e))).ToArray());
    }
    public async Task<UserDto?> GetUserByCredentials(UserCredentialsDto dto, CancellationToken cancellationToken = default)
    {
        var user = await manager.FindByEmailAsync(dto.Email);
        if (user is null) return null;
        var result = await signIn.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        return result.Succeeded ? new(user.Id, user.Email!, user.FirstName, user.LastName) : null;
    }
    private static string Translate(IdentityError error) => error.Code switch
    {
        "DuplicateEmail" or "DuplicateUserName" => "Este correo electrónico ya está registrado.",
        "PasswordTooShort" => "La contraseña debe tener al menos 6 caracteres.",
        "PasswordRequiresLower" => "La contraseña debe incluir una letra minúscula.",
        "PasswordRequiresUpper" => "La contraseña debe incluir una letra mayúscula.",
        "PasswordRequiresDigit" => "La contraseña debe incluir un número.",
        "PasswordRequiresNonAlphanumeric" => "La contraseña debe incluir un símbolo.",
        "InvalidEmail" or "InvalidUserName" => "Ingresa un correo electrónico válido.",
        "PasswordRequiresUniqueChars" => "La contraseña debe contener más caracteres diferentes.",
        _ => "No se pudo crear la cuenta. Revisa los datos e inténtalo nuevamente."
    };
}
