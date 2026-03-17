using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DotNetEnv;
using videogame_randomized_back.Models;

namespace videogame_randomized_back.Services;

public class EmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiToken;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(
        ILogger<EmailService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;

        // Load environment variables from .env file
        Env.Load();

        _apiToken = Environment.GetEnvironmentVariable("MAILTRAP_TOKEN")
            ?? throw new InvalidOperationException("MAILTRAP_TOKEN environment variable is not set");

        _fromEmail = Environment.GetEnvironmentVariable("MAILTRAP_FROM_EMAIL")
            ?? throw new InvalidOperationException("MAILTRAP_FROM_EMAIL environment variable is not set");

        _fromName = Environment.GetEnvironmentVariable("MAILTRAP_FROM_NAME") ?? "VideoGame Randomizer";
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiToken);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var payload = new
            {
                from = new { email = _fromEmail, name = _fromName },
                to = new[] { new { email = toEmail } },
                subject,
                html = htmlBody,
                category = "Transactional"
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://send.api.mailtrap.io/api/send", content);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Email sent to {Email} with subject '{Subject}'", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }

    public async Task SendConfirmationEmailAsync(string toEmail, string confirmationLink)
    {
        var subject = "Confirm Your Email - VideoGame Randomizer";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background-color: #09090b; color: #d4d4d8; padding: 40px; border-radius: 12px;'>
                <div style='text-align: center; margin-bottom: 30px;'>
                    <h1 style='color: #22d3ee; margin: 0;'>VideoGame <span style='color: #d946ef;'>Randomizer</span></h1>
                </div>
                <h2 style='color: #ffffff; text-align: center;'>Confirm Your Email</h2>
                <p style='text-align: center; color: #a1a1aa;'>Click the button below to verify your email address and activate your account.</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{confirmationLink}' 
                       style='background-color: #22d3ee; color: #09090b; padding: 14px 32px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 16px; display: inline-block;'>
                        Confirm Email
                    </a>
                </div>
                <p style='color: #71717a; font-size: 12px; text-align: center;'>If you didn't create an account, you can safely ignore this email.</p>
            </div>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var subject = "Reset Your Password - VideoGame Randomizer";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background-color: #09090b; color: #d4d4d8; padding: 40px; border-radius: 12px;'>
                <div style='text-align: center; margin-bottom: 30px;'>
                    <h1 style='color: #22d3ee; margin: 0;'>VideoGame <span style='color: #d946ef;'>Randomizer</span></h1>
                </div>
                <h2 style='color: #ffffff; text-align: center;'>Reset Your Password</h2>
                <p style='text-align: center; color: #a1a1aa;'>Click the button below to reset your password. This link will expire in 24 hours.</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{resetLink}' 
                       style='background-color: #d946ef; color: #09090b; padding: 14px 32px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 16px; display: inline-block;'>
                        Reset Password
                    </a>
                </div>
                <p style='color: #71717a; font-size: 12px; text-align: center;'>If you didn't request a password reset, you can safely ignore this email.</p>
            </div>";

        await SendEmailAsync(toEmail, subject, body);
    }
}
