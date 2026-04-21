using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using videogame_randomized_back.DTOs;
using videogame_randomized_back.Infrastructure;
using videogame_randomized_back.Services;

namespace videogame_randomized_back.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    AuthService authService,
    EmailService emailService,
    ILogger<AuthController> logger) : ControllerBase
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
            return Problem(
                title: "Registration failed",
                detail: string.Join("; ", result.Errors ?? []),
                statusCode: StatusCodes.Status400BadRequest);
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

        try
        {
            await emailService.SendConfirmationEmailAsync(dto.Email, confirmationLink);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Registration succeeded but confirmation email was not sent for {Email}", dto.Email);
            return Ok(new
            {
                message =
                    "Registration successful, but we could not send the confirmation email. Check Mailtrap credentials (EMAIL_API_TOKEN) or try again later.",
                confirmationEmailSent = false
            });
        }

        return Ok(new
        {
            message = "Registration successful. Please check your email to confirm your account.",
            confirmationEmailSent = true
        });
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
            return Problem(
                title: "Login failed",
                detail: result.Error,
                type: result.EmailNotConfirmed ? AuthProblemTypes.LoginEmailNotConfirmed : null,
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Ok(new AuthResponseDto(result.Token!, result.Email!));
    }

    /// <summary>
    /// Resends the email confirmation link for an existing account.
    /// </summary>
    [HttpPost("resend-confirmation")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> ResendConfirmation([FromBody] ResendConfirmationDto dto)
    {
        var frontendBaseUrl = Request.Headers.Origin.FirstOrDefault()
                              ?? "http://localhost:5173";

        var result = await authService.ResendConfirmationEmailAsync(dto.Email, dto.Password, frontendBaseUrl);
        if (!result.IsSuccess)
        {
            return Problem(
                title: "Resend confirmation failed",
                detail: result.Error,
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Ok(new
        {
            message = "If the account exists and is not confirmed, a confirmation link has been sent.",
            confirmationEmailSent = result.ConfirmationEmailSent
        });
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
            return Problem(
                title: "Email confirmation failed",
                detail: result.Error,
                statusCode: StatusCodes.Status400BadRequest);
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

        try
        {
            await emailService.SendPasswordResetEmailAsync(dto.Email, resetLink);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Password reset email was not sent for {Email}", dto.Email);
        }

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
            return Problem(
                title: "Password reset failed",
                detail: result.Error,
                statusCode: StatusCodes.Status400BadRequest);
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
            return Problem(
                title: "Unauthorized",
                detail: "User not found",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var result = await authService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
        if (!result.IsSuccess)
        {
            return Problem(
                title: "Password change failed",
                detail: result.Error,
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Ok(new { message = "Password changed successfully." });
    }
}
