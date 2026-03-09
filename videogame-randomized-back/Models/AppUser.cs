using Microsoft.AspNetCore.Identity;

namespace videogame_randomized_back.Models;

public class AppUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
