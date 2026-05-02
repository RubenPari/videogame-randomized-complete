using videogame_randomized_back.Models;

namespace videogame_randomized_back.Services;

public interface IJwtTokenService
{
    Task<string> GenerateAsync(AppUser user);
}

