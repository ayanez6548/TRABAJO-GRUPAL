using NorthWind.Validation.Entities;
namespace NorthWind.Result.Entities;

public sealed class Result<T>
{
    private Result(T? value, IReadOnlyList<ValidationError> errors) { Value = value; Errors = errors; }
    public T? Value { get; }
    public IReadOnlyList<ValidationError> Errors { get; }
    public bool HasError => Errors.Count > 0;
    public static Result<T> Success(T value) => new(value, []);
    public static Result<T> Failure(params ValidationError[] errors) => new(default, errors);
    public static Result<T> Failure(IReadOnlyList<ValidationError> errors) => new(default, errors);
}
