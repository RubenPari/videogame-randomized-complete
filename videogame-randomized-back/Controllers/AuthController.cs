using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using videogame_randomized_back.DTOs;
using videogame_randomized_back.Services;

namespace videogame_randomized_back.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService authService, EmailService emailService) : ControllerBase
{
    /// <summary>
    /// Registers a new user account
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> Register([FromBody] RegisterDto dto)
    {
        var result = await authService.RegisterAsync(dto.Email, dto.Password);
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        var token = await authService.GenerateEmailConfirmationTokenAsync(dto.Email);
        var userId = await authService.GetUserIdByEmailAsync(dto.Email);

        if (token == null || userId == null)
        {
            return Ok(new { message = "Registration successful. Please check your email to confirm your account." });
        }

        var encodedToken = Uri.EscapeDataString(token);
        var frontendBaseUrl = Request.Headers.Origin.FirstOrDefault()
                              ?? "http://localhost:5173";
        
        var confirmationLink = $"{frontendBaseUrl}/confirm-email?userId={userId}&token={encodedToken}";

        await emailService.SendConfirmationEmailAsync(dto.Email, confirmationLink);

        return Ok(new { message = "Registration successful. Please check your email to confirm your account." });
    }

    /// <summary>
    /// Logs in an existing user
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var result = await authService.LoginAsync(dto.Email, dto.Password);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new AuthResponseDto());
    }

    /// <summary>
    /// Confirms a user's email address
    /// </summary>
    [HttpGet("confirm-email")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var decodedToken = Uri.UnescapeDataString(token);
        var result = await authService.ConfirmEmailAsync(userId, decodedToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Email confirmed successfully. You can now log in." });
    }

    /// <summary>
    /// Sends a password reset email to the user
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var resetToken = await authService.GeneratePasswordResetTokenAsync(dto.Email);
        var userId = await authService.GetUserIdByEmailAsync(dto.Email);

        if (resetToken == null || userId == null)
        {
            return Ok(new { message = "If the email exists, a password reset link has been sent." });
        }

        var encodedToken = Uri.EscapeDataString(resetToken);
        var frontendBaseUrl = Request.Headers.Origin.FirstOrDefault()
                              ?? "http://localhost:5173";
        
        var resetLink = $"{frontendBaseUrl}/reset-password?userId={userId}&token={encodedToken}";

        await emailService.SendPasswordResetEmailAsync(dto.Email, resetLink);

        return Ok(new { message = "If the email exists, a password reset link has been sent." });
    }

    /// <summary>
    /// Resets the user's password using a token
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var decodedToken = Uri.UnescapeDataString(dto.Token);
        var result = await authService.ResetPasswordAsync(dto.UserId, decodedToken, dto.NewPassword);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Password reset successfully. You can now log in." });
    }

    /// <summary>
    /// Changes the current user's password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new { error = "User not found" });
        }

        var result = await authService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Password changed successfully." });
    }
}
