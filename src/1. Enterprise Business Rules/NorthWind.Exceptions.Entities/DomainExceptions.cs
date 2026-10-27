using NorthWind.Validation.Entities;
namespace NorthWind.Exceptions.Entities;

public sealed class ValidationException(IReadOnlyList<ValidationError> errors)
    : Exception("Revisa los datos ingresados.")
{
    public IReadOnlyList<ValidationError> Errors { get; } = errors;
}

public sealed class UpdateException(string message, Exception? innerException = null) : Exception(message, innerException);
public sealed class NotificationException(string message, Exception? innerException = null) : Exception(message, innerException);
public sealed class StockConflictException(string message) : Exception(message);
