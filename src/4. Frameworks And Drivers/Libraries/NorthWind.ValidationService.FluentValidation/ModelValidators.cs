using FluentValidation;
using NorthWind.Validation.Entities;
using NorthWind.Membership.Entities.Dtos;
using System.Net.Mail;
namespace NorthWind.ValidationService.FluentValidation;

public abstract class FluentModelValidator<T> : AbstractValidator<T>, IModelValidator<T>
{
    public ValidationConstraint Constraint => ValidationConstraint.AlwaysValidate;
    async Task<IReadOnlyList<ValidationError>> IModelValidator<T>.Validate(T model, CancellationToken cancellationToken)
    {
        var result = await ValidateAsync(model, cancellationToken);
        return result.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)).ToArray();
    }
}
public sealed class UserRegistrationDtoValidator : FluentModelValidator<UserRegistrationDto>
{
    public UserRegistrationDtoValidator()
    {
        RuleFor(x => x.FirstName).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Ingresa tus nombres.")
            .MaximumLength(MembershipFieldLimits.Name).WithMessage("Los nombres admiten hasta 30 caracteres.")
            .Matches(@"^ *\p{L}[\p{L}\p{M} ]*$").WithMessage("Los nombres solo admiten letras y espacios, incluidas tildes y ñ.");
        RuleFor(x => x.LastName).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Ingresa tus apellidos.")
            .MaximumLength(MembershipFieldLimits.Name).WithMessage("Los apellidos admiten hasta 30 caracteres.")
            .Matches(@"^ *\p{L}[\p{L}\p{M} ]*$").WithMessage("Los apellidos solo admiten letras y espacios, incluidas tildes y ñ.");
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Ingresa tu correo electrónico.")
            .MaximumLength(MembershipFieldLimits.Email).WithMessage("El correo electrónico admite hasta 30 caracteres.")
            .Must(EmailRules.IsValid).WithMessage("Ingresa un correo electrónico válido, por ejemplo nombre@dominio.com.");
        RuleFor(x => x.Password).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Ingresa tu contraseña.")
            .MinimumLength(MembershipFieldLimits.MinimumPassword).WithMessage("La contraseña debe tener al menos 6 caracteres.")
            .Must(p => p.Any(char.IsLower)).WithMessage("Incluye una letra minúscula.")
            .Must(p => p.Any(char.IsUpper)).WithMessage("Incluye una letra mayúscula.")
            .Must(p => p.Any(char.IsDigit)).WithMessage("Incluye un número.")
            .Must(p => p.Any(c => !char.IsLetterOrDigit(c))).WithMessage("Incluye un símbolo.");
    }
}
public sealed class UserCredentialsDtoValidator : FluentModelValidator<UserCredentialsDto>
{
    public UserCredentialsDtoValidator()
    {
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Ingresa tu correo electrónico.")
            .MaximumLength(MembershipFieldLimits.Email).WithMessage("El correo electrónico admite hasta 30 caracteres.")
            .Must(EmailRules.IsValid).WithMessage("Ingresa un correo electrónico válido, por ejemplo nombre@dominio.com.");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Ingresa tu contraseña.");
    }
}

internal static class EmailRules
{
    public static bool IsValid(string value)
    {
        if (string.IsNullOrEmpty(value) || !value.All(MembershipEmailPolicy.AllowedCharacters.Contains)) return false;
        var parts = value.Split('@');
        if (parts.Length != 2) return false;
        var local = parts[0];
        if (local.Length is < 1 or > 64 || local.StartsWith('.') || local.EndsWith('.') || local.Contains("..")) return false;
        var domain = parts[1];
        var labels = domain.Split('.');
        if (domain.Length > 253 || labels.Length < 2 || labels.Any(label =>
            label.Length is < 1 or > 63 || label.StartsWith('-') || label.EndsWith('-')
            || label.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '-'))) return false;
        return MailAddress.TryCreate(value, out var address)
            && address.Address.Equals(value, StringComparison.OrdinalIgnoreCase);
    }
}
