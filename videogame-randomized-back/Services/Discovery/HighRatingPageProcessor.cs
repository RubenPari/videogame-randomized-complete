using System.Text.Json;

namespace videogame_randomized_back.Services.Discovery;

internal static class HighRatingPageProcessor
{
    /// <summary>
    /// Primary high-rating scan: keeps games rated &gt;= minRating and exposes the first numeric rating on the page for early exit.
    /// </summary>
    public static void CollectFromScanPage(
        JsonElement resultsEl,
        decimal minRating,
        HashSet<int> seen,
        List<JsonElement> pool,
        ref bool anyAbove,
        out decimal? firstRatedOnPage)
    {
        firstRatedOnPage = null;

        foreach (var g in resultsEl.EnumerateArray())
        {
            if (!RawgDiscoveryJson.TryGetInt(g, "id", out var id)) continue;
            var rating = RawgDiscoveryJson.TryGetDecimal(g, "rating");
            if (!rating.HasValue) continue;

            firstRatedOnPage ??= rating.Value;

            if (rating.Value >= minRating)
            {
                anyAbove = true;
                if (seen.Add(id))
                    pool.Add(g.Clone());
            }
        }
    }

    /// <summary>
    /// Fallback pages after the initial scan window (same semantics as the original loop body).
    /// </summary>
    public static void CollectFromFallbackPage(
        JsonElement resultsEl,
        decimal minRating,
        HashSet<int> seen,
        List<JsonElement> pool,
        ref bool anyAbove)
    {
        foreach (var g in resultsEl.EnumerateArray())
        {
            if (!RawgDiscoveryJson.TryGetInt(g, "id", out var id)) continue;
            var rating = RawgDiscoveryJson.TryGetDecimal(g, "rating");
            if (!rating.HasValue || rating.Value < minRating) continue;

            anyAbove = true;
            if (seen.Add(id))
                pool.Add(g.Clone());
        }
    }
}
