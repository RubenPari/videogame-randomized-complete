namespace videogame_randomized_back.Models;

public class SmtpSettings
{
    public string ApiToken { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "VideoGame Randomizer";
}
