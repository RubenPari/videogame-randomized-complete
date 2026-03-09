namespace videogame_randomized_back.DTOs;

public record RegisterDto(string Email, string Password, string ConfirmPassword);

public record LoginDto(string Email, string Password);

public record ForgotPasswordDto(string Email);

public record ResetPasswordDto(string UserId, string Token, string NewPassword);

public record ChangePasswordDto(string CurrentPassword, string NewPassword);

public record AuthResponseDto(string Token, string Email);
