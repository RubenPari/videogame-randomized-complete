using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using videogame_randomized_back.DTOs;
using videogame_randomized_back.Models;

namespace videogame_randomized_back.Services;

/// <summary>
/// Service for handling authentication operations including user registration,
/// login, email confirmation, password reset, and JWT token generation.
/// </summary>
public class AuthService(
    UserManager<AppUser> userManager, 
    IOptions<JwtSettings> jwtSettings,
    ILogger<AuthService> logger)
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user to generate the token for.</param>
    /// <returns>A JWT token string.</returns>
    private async Task<string> GenerateJwtTokenAsync(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        
        return handler.WriteToken(token);
    }

    /// <summary>
    /// Registers a new user with the specified email and password.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>A RegistrationResult containing success status, email, and any errors.</returns>
    public async Task<RegistrationResult> RegisterAsync(string email, string password)
    {
        var user = new AppUser
        {
            UserName = email,
            Email = email
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger.LogWarning("Registrazione fallita per l'email: {Email}", email);
            return RegistrationResult.Failed(result.Errors.Select(e => e.Description));
        }

        logger.LogInformation("Nuovo utente registrato con successo: {Email}", email);
        return RegistrationResult.Succeeded(user.Email);
    }

    /// <summary>
    /// Generates an email confirmation token for the specified user.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <returns>A confirmation token string, or null if the user is not found.</returns>
    public async Task<string?> GenerateEmailConfirmationTokenAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null) return null;

        return await userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    /// <summary>
    /// Confirms the user's email address using the provided token.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="token">The confirmation token.</param>
    /// <returns>A ConfirmationResult containing success status and any error message.</returns>
    public async Task<ConfirmationResult> ConfirmEmailAsync(string userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) 
        {
            logger.LogWarning("Conferma email fallita: Utente {UserId} non trovato", userId);
            return ConfirmationResult.Failed("User not found");
        }

        var result = await userManager.ConfirmEmailAsync(user, token);
        
        if (!result.Succeeded)
        {
            return ConfirmationResult.Failed(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        logger.LogInformation("Email confermata per l'utente: {UserId}", userId);
        return ConfirmationResult.Succeeded();
    }

    /// <summary>
    /// Authenticates a user with the specified email and password.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>A LoginResult containing success status, JWT token, email, and any error message.</returns>
    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            logger.LogWarning("Tentativo di login fallito: Email {Email} non trovata", email);
            return LoginResult.Failed("Invalid email or password");
        }

        if (!await userManager.IsEmailConfirmedAsync(user))
        {
            logger.LogInformation("Tentativo di login per email non confermata: {Email}", email);
            return LoginResult.Failed("Please confirm your email before logging in");
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, password);
        if (!isPasswordValid)
        {
            logger.LogWarning("Tentativo di login fallito: Password errata per {Email}", email);
            return LoginResult.Failed("Invalid email or password");
        }

        var token = await GenerateJwtTokenAsync(user);
        
        logger.LogInformation("Login effettuato con successo: {Email}", email);
        return LoginResult.Succeeded(token, user.Email!);
    }

    /// <summary>
    /// Generates a password reset token for the specified user.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <returns>A password reset token string, or null if the user is not found.</returns>
    public async Task<string?> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null) return null;

        return await userManager.GeneratePasswordResetTokenAsync(user);
    }

    /// <summary>
    /// Retrieves the user ID associated with the specified email address.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <returns>The user ID, or null if not found.</returns>
    public async Task<string?> GetUserIdByEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user?.Id;
    }

    /// <summary>
    /// Resets the user's password using the provided token.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="token">The password reset token.</param>
    /// <param name="newPassword">The new password.</param>
    /// <returns>An AuthResult containing success status and any error message.</returns>
    public async Task<AuthResult> ResetPasswordAsync(string userId, string token, string newPassword)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return AuthResult.Failure("User not found");

        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        
        if (!result.Succeeded)
        {
            logger.LogWarning("Reset password fallito per l'utente {UserId}", userId);
            return AuthResult.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        logger.LogInformation("Password resettata con successo per l'utente {UserId}", userId);
        return AuthResult.Success();
    }

    /// <summary>
    /// Changes the user's password by verifying the current password first.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="currentPassword">The current password.</param>
    /// <param name="newPassword">The new password.</param>
    /// <returns>An AuthResult containing success status and any error message.</returns>
    public async Task<AuthResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return AuthResult.Failure("User not found");

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        
        if (!result.Succeeded)
        {
            logger.LogWarning("Cambio password fallito per l'utente {UserId}", userId);
            return AuthResult.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        logger.LogInformation("Password cambiata con successo per l'utente {UserId}", userId);
        return AuthResult.Success();
    }
}