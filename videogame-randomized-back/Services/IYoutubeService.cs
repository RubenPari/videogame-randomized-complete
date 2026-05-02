namespace videogame_randomized_back.Services;

public interface IYoutubeService
{
    bool IsConfigured { get; }

    Task<(int StatusCode, string Body)> SearchAsync(string query);
}

