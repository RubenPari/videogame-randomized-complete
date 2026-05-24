using videogame_randomized_back.DTOs;

namespace videogame_randomized_back.Services;

public interface IAuthService
{
    Task<RegistrationResult> RegisterAsync(string email, string password);
    Task<string?> GenerateEmailConfirmationTokenAsync(string email);
    Task<ConfirmationResult> ConfirmEmailAsync(string userId, string token);
    Task<LoginResult> LoginAsync(string email, string password);
    Task<ResendConfirmationResult> ResendConfirmationEmailAsync(string email, string password, string frontendBaseUrl);
    Task<string?> GeneratePasswordResetTokenAsync(string email);
    Task<string?> GetUserIdByEmailAsync(string email);
    Task<AuthResult> ResetPasswordAsync(string userId, string token, string newPassword);
    Task<AuthResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
}
