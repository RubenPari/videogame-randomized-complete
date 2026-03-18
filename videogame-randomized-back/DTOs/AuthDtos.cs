namespace videogame_randomized_back.DTOs;

public abstract record RegisterDto(string Email, string Password, string ConfirmPassword);

public abstract record LoginDto(string Email, string Password);

public abstract record ForgotPasswordDto(string Email);

public abstract record ResetPasswordDto(string UserId, string Token, string NewPassword);

public abstract record ChangePasswordDto(string CurrentPassword, string NewPassword);

public record AuthResponseDto;
