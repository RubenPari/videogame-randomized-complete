namespace videogame_randomized_back.Services;

public interface IEmailService
{
    Task SendConfirmationEmailAsync(string toEmail, string confirmationLink);
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
}
