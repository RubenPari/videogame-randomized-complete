using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace videogame_randomized_back.Services;

public class TranslateService(
    ILogger<TranslateService> logger,
    IHttpClientFactory httpClientFactory)
{
    private readonly ILogger<TranslateService> _logger = logger;
    private readonly HttpClient _client = httpClientFactory.CreateClient("googleTranslate");

    private static string? ApiKey => Environment.GetEnvironmentVariable("GOOGLE_TRANSLATE_API_KEY");

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);

    public async Task<(int StatusCode, string Body)> TranslateAsync(string text, string source, string target)
    {
        var apiKey = ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("GOOGLE_TRANSLATE_API_KEY is not configured");
        }

        var payload = new
        {
            q = text,
            source,
            target,
            format = "text"
        };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            using var response = await _client.PostAsync($"language/translate/v2?key={Uri.EscapeDataString(apiKey)}", content);
            var body = await response.Content.ReadAsStringAsync();
            return ((int)response.StatusCode, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google Translate proxy request failed");
            throw;
        }
    }
}

