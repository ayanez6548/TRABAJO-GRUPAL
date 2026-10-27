namespace NorthWind.Validation.Entities;

public sealed record ValidationError(string PropertyName, string Message);
public enum ValidationConstraint { AlwaysValidate, ValidateIfThereAreNoPreviousErrors }

public interface IModelValidator<T>
{
    ValidationConstraint Constraint { get; }
    Task<IReadOnlyList<ValidationError>> Validate(T model, CancellationToken cancellationToken = default);
}

public interface IModelValidatorHub<T>
{
    Task<IReadOnlyList<ValidationError>> Validate(T model, CancellationToken cancellationToken = default);
}

public sealed class ModelValidatorHub<T>(IEnumerable<IModelValidator<T>> validators) : IModelValidatorHub<T>
{
    public async Task<IReadOnlyList<ValidationError>> Validate(T model, CancellationToken cancellationToken = default)
    {
        List<ValidationError> errors = [];
        foreach (var validator in validators.OrderBy(v => v.Constraint))
        {
            if (errors.Count > 0 && validator.Constraint == ValidationConstraint.ValidateIfThereAreNoPreviousErrors)
                continue;
            errors.AddRange(await validator.Validate(model, cancellationToken));
        }
        return errors;
    }
}
