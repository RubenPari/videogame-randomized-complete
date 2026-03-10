using System.ComponentModel.DataAnnotations;

namespace videogame_randomized_back.Models;

public class DiscoveryLogEntry
{
    [Key]
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;

    public int GameExternalId { get; set; }
    public string GameName { get; set; } = string.Empty;

    public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;
}
