namespace videogame_randomized_back.Services;

public interface ITranslateService
{
    bool IsConfigured { get; }

    Task<(int StatusCode, string Body)> TranslateAsync(string text, string source, string target);
}

