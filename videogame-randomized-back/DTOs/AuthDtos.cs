namespace videogame_randomized_back.DTOs;

public abstract record RegisterDto(string Email, string Password, string ConfirmPassword);

public abstract record LoginDto(string Email, string Password);

public abstract record ForgotPasswordDto(string Email);

public abstract record ResetPasswordDto(string UserId, string Token, string NewPassword);

public abstract record ChangePasswordDto(string CurrentPassword, string NewPassword);

public record AuthResponseDto;

/// <summary>
/// Base result type for authentication operations.
/// </summary>
public record AuthResult(bool IsSuccess, string? Error = null)
{
    public static AuthResult Success() => new(true);
    public static AuthResult Failure(string error) => new(false, error);
}

/// <summary>
/// Result for registration operations.
/// </summary>
public record RegistrationResult(
    bool IsSuccess, 
    string? Email = null, 
    IEnumerable<string>? Errors = null)
{
    public static RegistrationResult Succeeded(string email) => new(true, email);
    public static RegistrationResult Failed(IEnumerable<string> errors) => new(false, null, errors);
}

/// <summary>
/// Result for login operations.
/// </summary>
public record LoginResult(
    bool IsSuccess, 
    string? Token = null, 
    string? Email = null, 
    string? Error = null)
{
    public static LoginResult Succeeded(string token, string email) => new(true, token, email);
    public static LoginResult Failed(string error) => new(false, null, null, error);
}

/// <summary>
/// Result for email confirmation operations.
/// </summary>
public record ConfirmationResult(bool IsSuccess, string? Error = null)
{
    public static ConfirmationResult Succeeded() => new(true);
    public static ConfirmationResult Failed(string error) => new(false, error);
}
