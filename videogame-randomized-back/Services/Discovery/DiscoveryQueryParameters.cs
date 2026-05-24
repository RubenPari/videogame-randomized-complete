using videogame_randomized_back.DTOs;

namespace videogame_randomized_back.Services.Discovery;

internal static class DiscoveryQueryParameters
{
    public static Dictionary<string, string?> BuildBase(DiscoveryRandomRequestDto request)
    {
        var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(request.Genre)) dict["genres"] = request.Genre;
        if (!string.IsNullOrWhiteSpace(request.Platforms)) dict["platforms"] = request.Platforms;

        var startYear = request.StartYear.GetValueOrDefault(2010);
        var endDate = request.EndYear.HasValue
            ? new DateTime(request.EndYear.Value, 12, 31)
            : DateTime.UtcNow;
        dict["dates"] = $"{startYear:D4}-01-01,{endDate:yyyy-MM-dd}";

        if (request.ExcludeAdditions.GetValueOrDefault(true))
            dict["exclude_additions"] = "true";

        return dict;
    }
}
