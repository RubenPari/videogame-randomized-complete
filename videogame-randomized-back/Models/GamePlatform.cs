namespace videogame_randomized_back.Models;

public class GamePlatform
{
    public int GameId { get; set; }
    public Game Game { get; set; } = null!;
    
    public int PlatformId { get; set; }
    public Platform Platform { get; set; } = null!;
}
