namespace videogame_randomized_back.Models;

public class EmailSettings
{
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "VideoGame Randomizer";
    public string? ApiToken { get; set; }
}
