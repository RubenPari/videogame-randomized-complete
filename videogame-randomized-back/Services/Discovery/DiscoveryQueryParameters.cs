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
        var endYear = request.EndYear.GetValueOrDefault(DateTime.UtcNow.Year);
        dict["dates"] = $"{startYear:D4}-01-01,{endYear:D4}-12-31";

        return dict;
    }
}
