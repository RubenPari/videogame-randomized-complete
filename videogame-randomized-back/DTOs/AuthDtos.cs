namespace videogame_randomized_back.DTOs;

public record RegisterDto(string Email, string Password, string ConfirmPassword);

public record LoginDto(string Email, string Password);

public record ForgotPasswordDto(string Email);

public record ResetPasswordDto(string UserId, string Token, string NewPassword);

public record ChangePasswordDto(string CurrentPassword, string NewPassword);

public record AuthResponseDto;

public record AuthResult
{
    public bool IsSuccess { get; private init; }
    public string? Error { get; private init; }

    public static AuthResult Success() => new() { IsSuccess = true };
    public static AuthResult Failure(string error) => new() { IsSuccess = false, Error = error };
}

public record RegistrationResult
{
    public bool IsSuccess { get; private init; }
    public string? Email { get; init; }
    public IEnumerable<string>? Errors { get; private init; }

    public static RegistrationResult Succeeded(string email) => new() { IsSuccess = true, Email = email };
    public static RegistrationResult Failed(IEnumerable<string> errors) => new() { IsSuccess = false, Errors = errors };
}

public record LoginResult
{
    public bool IsSuccess { get; private init; }
    public string? Token { get; init; }
    public string? Email { get; init; }
    public string? Error { get; private init; }

    public static LoginResult Succeeded(string token, string email) => new() { IsSuccess = true, Token = token, Email = email };
    public static LoginResult Failed(string error) => new() { IsSuccess = false, Error = error };
}

public record ConfirmationResult
{
    public bool IsSuccess { get; private init; }
    public string? Error { get; private init; }

    public static ConfirmationResult Succeeded() => new() { IsSuccess = true };
    public static ConfirmationResult Failed(string error) => new() { IsSuccess = false, Error = error };
}
