using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace videogame_randomized_back.Services;

public class RawgService(
    ILogger<RawgService> logger,
    IHttpClientFactory httpClientFactory)
{
    private readonly ILogger<RawgService> _logger = logger;
    private readonly HttpClient _client = httpClientFactory.CreateClient("rawg");

    private static string? ApiKey => Environment.GetEnvironmentVariable("RAWG_API_KEY");

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);

    public async Task<(int StatusCode, string Body)> GetAsync(string rawgPath, IQueryCollection query)
    {
        var apiKey = ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("RAWG_API_KEY is not configured");
        }

        var url = QueryHelpers.AddQueryString(rawgPath.TrimStart('/'), ToQueryDictionary(query, apiKey));

        try
        {
            using var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            var body = await response.Content.ReadAsStringAsync();
            return ((int)response.StatusCode, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RAWG proxy request failed for {Url}", url);
            throw;
        }
    }

    private static Dictionary<string, string?> ToQueryDictionary(
        IQueryCollection query,
        string apiKey)
    {
        var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["key"] = apiKey
        };

        foreach (var (k, v) in query)
        {
            if (string.Equals(k, "key", StringComparison.OrdinalIgnoreCase)) continue;
            if (v.Count == 0) continue;

            // RAWG accepts standard query keys; for multi-values we forward comma-joined.
            dict[k] = string.Join(",", v.ToArray());
        }

        return dict;
    }
}
