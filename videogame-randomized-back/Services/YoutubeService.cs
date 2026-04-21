using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace videogame_randomized_back.Services;

public class YoutubeService(
    ILogger<YoutubeService> logger,
    IHttpClientFactory httpClientFactory)
{
    private readonly ILogger<YoutubeService> _logger = logger;
    private readonly HttpClient _client = httpClientFactory.CreateClient("youtube");

    private static string? ApiKey => Environment.GetEnvironmentVariable("YOUTUBE_API_KEY");

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);

    public async Task<(int StatusCode, string Body)> SearchAsync(string query)
    {
        var apiKey = ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("YOUTUBE_API_KEY is not configured");
        }

        var qs = new Dictionary<string, string?>
        {
            ["part"] = "snippet",
            ["q"] = query,
            ["type"] = "video",
            ["maxResults"] = "1",
            ["videoCategoryId"] = "20",
            ["key"] = apiKey
        };

        var url = QueryHelpers.AddQueryString("search", qs);

        try
        {
            using var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            var body = await response.Content.ReadAsStringAsync();
            return ((int)response.StatusCode, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "YouTube proxy request failed for query {Query}", query);
            throw;
        }
    }
}

