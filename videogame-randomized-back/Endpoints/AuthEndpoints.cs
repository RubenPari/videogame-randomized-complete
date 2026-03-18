using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using videogame_randomized_back.DTOs;
using videogame_randomized_back.Filters;
using videogame_randomized_back.Services;

namespace videogame_randomized_back.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/auth")
            .WithTags("Authentication");

        group.MapPost("/register", Register)
            .AddEndpointFilter<ValidationFilter<RegisterDto>>();

        group.MapPost("/login", Login)
            .AddEndpointFilter<ValidationFilter<LoginDto>>();

        group.MapGet("/confirm-email", ConfirmEmail);

        group.MapPost("/forgot-password", ForgotPassword)
            .AddEndpointFilter<ValidationFilter<ForgotPasswordDto>>();

        group.MapPost("/reset-password", ResetPassword)
            .AddEndpointFilter<ValidationFilter<ResetPasswordDto>>();

        group.MapPost("/change-password", ChangePassword)
            .AddEndpointFilter<ValidationFilter<ChangePasswordDto>>()
            .RequireAuthorization();
    }

    private static async Task<Results<Ok<object>, BadRequest<object>>> Register(
        RegisterDto dto,
        AuthService authService,
        EmailService emailService,
        HttpContext httpContext)
    {
        var result = await authService.RegisterAsync(dto.Email, dto.Password);
        if (!result.Success)
        {
            return TypedResults.BadRequest((object)new { errors = result.Errors });
        }

        // Generate email confirmation token and send email
        var token = await authService.GenerateEmailConfirmationTokenAsync(dto.Email);
        var userId = await authService.GetUserIdByEmailAsync(dto.Email);

        if (token == null || userId == null)
            return TypedResults.Ok((object)new
                { message = "Registration successful. Please check your email to confirm your account." });
        
        var encodedToken = Uri.EscapeDataString(token);
        // Build the confirmation link pointing to the frontend
        var frontendBaseUrl = httpContext.Request.Headers.Origin.FirstOrDefault()
                              ?? "http://localhost:5173";
        var confirmationLink = $"{frontendBaseUrl}/confirm-email?userId={userId}&token={encodedToken}";

        await emailService.SendConfirmationEmailAsync(dto.Email, confirmationLink);

        return TypedResults.Ok((object)new { message = "Registration successful. Please check your email to confirm your account." });
    }

    private static async Task<Results<Ok<AuthResponseDto>, BadRequest<object>>> Login(
        LoginDto dto,
        AuthService authService)
    {
        var result = await authService.LoginAsync(dto.Email, dto.Password);
        if (!result.Success)
        {
            return TypedResults.BadRequest((object)new { error = result.Error });
        }

        return TypedResults.Ok(new AuthResponseDto());
    }

    private static async Task<Results<Ok<object>, BadRequest<object>>> ConfirmEmail(
        [FromQuery] string userId,
        [FromQuery] string token,
        AuthService authService)
    {
        var decodedToken = Uri.UnescapeDataString(token);
        var result = await authService.ConfirmEmailAsync(userId, decodedToken);

        if (!result.Success)
        {
            return TypedResults.BadRequest((object)new { error = result.Error });
        }

        return TypedResults.Ok((object)new { message = "Email confirmed successfully. You can now log in." });
    }

    private static async Task<Ok<object>> ForgotPassword(
        ForgotPasswordDto dto,
        AuthService authService,
        EmailService emailService,
        HttpContext httpContext)
    {
        var resetToken = await authService.GeneratePasswordResetTokenAsync(dto.Email);
        var userId = await authService.GetUserIdByEmailAsync(dto.Email);

        if (resetToken == null || userId == null)
            return TypedResults.Ok(
                (object)new { message = "If the email exists, a password reset link has been sent." });
        
        var encodedToken = Uri.EscapeDataString(resetToken);
        var frontendBaseUrl = httpContext.Request.Headers.Origin.FirstOrDefault()
                              ?? "http://localhost:5173";
        var resetLink = $"{frontendBaseUrl}/reset-password?userId={userId}&token={encodedToken}";

        await emailService.SendPasswordResetEmailAsync(dto.Email, resetLink);

        // Always return success to avoid leaking user info
        return TypedResults.Ok((object)new { message = "If the email exists, a password reset link has been sent." });
    }

    private static async Task<Results<Ok<object>, BadRequest<object>>> ResetPassword(
        ResetPasswordDto dto,
        AuthService authService)
    {
        var decodedToken = Uri.UnescapeDataString(dto.Token);
        var result = await authService.ResetPasswordAsync(dto.UserId, decodedToken, dto.NewPassword);

        if (!result.Success)
        {
            return TypedResults.BadRequest((object)new { error = result.Error });
        }

        return TypedResults.Ok((object)new { message = "Password reset successfully. You can now log in." });
    }

    private static async Task<Results<Ok<object>, BadRequest<object>>> ChangePassword(
        ChangePasswordDto dto,
        AuthService authService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return TypedResults.BadRequest((object)new { error = "User not found" });
        }

        var result = await authService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
        if (!result.Success)
        {
            return TypedResults.BadRequest((object)new { error = result.Error });
        }

        return TypedResults.Ok((object)new { message = "Password changed successfully." });
    }
}
