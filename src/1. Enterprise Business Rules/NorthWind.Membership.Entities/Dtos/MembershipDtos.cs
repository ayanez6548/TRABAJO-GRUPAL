namespace NorthWind.Membership.Entities.Dtos;
public sealed record UserRegistrationDto(string Email, string Password, string FirstName, string LastName);
public sealed record UserCredentialsDto(string Email, string Password);
public sealed record UserDto(string Id, string Email, string FirstName, string LastName);
public sealed record TokensDto(string AccessToken, DateTimeOffset ExpiresAt);

public static class MembershipFieldLimits
{
    public const int Name = 30;
    public const int Email = 30;
    public const int MinimumPassword = 6;
}

public static class MembershipEmailPolicy
{
    public const string AllowedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
}
