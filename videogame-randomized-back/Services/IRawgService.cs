using Microsoft.AspNetCore.Http;

namespace videogame_randomized_back.Services;

public interface IRawgService
{
    bool IsConfigured { get; }

    Task<(int StatusCode, string Body)> GetAsync(string rawgPath, IQueryCollection query);
}

