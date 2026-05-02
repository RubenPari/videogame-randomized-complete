using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using videogame_randomized_back.Data;
using videogame_randomized_back.DTOs;

namespace videogame_randomized_back.Services;

public class DiscoveryService(
    ILogger<DiscoveryService> logger,
    AppDbContext db,
    IRawgService rawg,
    IMemoryCache cache)
{
    private const int PageSize = 40;
    private const int MaxUniformFallbackPages = 3;
    private const int MaxHighRatingScanPages = 10;
    private const int HighRatingPoolThreshold = 100;
    private const int HighRatingFallbackPages = 5;
    private const int MaxUniformRepicks = 5;
    private const int MaxHighRatingRepicks = 5;
    private static readonly TimeSpan CountCacheTtl = TimeSpan.FromMinutes(5);

    private sealed class DiscoveryMetrics
    {
        public int RawgCalls { get; set; }
        public int Repicks { get; set; }
    }

    private record RawgPageResult(int StatusCode, string Body);

    public async Task<DiscoveryRandomResponseDto> GetRandomGameAsync(
        string userId,
        DiscoveryRandomRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!rawg.IsConfigured)
        {
            return new DiscoveryRandomResponseDto
            {
                Success = false,
                Error = "RAWG API key is not configured.",
                IsUpstreamFailure = false
            };
        }

        var minRating = request.MinRating.GetValueOrDefault(0m);
        var isHighRatingMode = minRating > 0m;
        var metrics = new DiscoveryMetrics();

        var excludedIds = await GetExcludedIdsAsync(userId, request.ExcludeIds, cancellationToken);

        if (isHighRatingMode)
        {
            var res = await GetRandomHighRatedAsync(request, excludedIds, metrics, cancellationToken);
            logger.LogInformation(
                "Discovery(highRating) rawgCalls={RawgCalls} repicks={Repicks} excludedCount={ExcludedCount}",
                metrics.RawgCalls,
                metrics.Repicks,
                excludedIds.Count);
            return res;
        }

        var result = await GetRandomUniformAsync(request, excludedIds, metrics, cancellationToken);
        logger.LogInformation(
            "Discovery(uniform) rawgCalls={RawgCalls} repicks={Repicks} excludedCount={ExcludedCount}",
            metrics.RawgCalls,
            metrics.Repicks,
            excludedIds.Count);
        return result;
    }

    private async Task<HashSet<int>> GetExcludedIdsAsync(
        string userId,
        string? excludeIds,
        CancellationToken cancellationToken)
    {
        var set = await db.DiscoveryLog
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .Select(e => e.GameExternalId)
            .ToHashSetAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(excludeIds))
            return set;

        foreach (var token in excludeIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(token, out var id))
                set.Add(id);
        }

        return set;
    }

    private static Dictionary<string, string?> BuildBaseParams(DiscoveryRandomRequestDto request)
    {
        var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(request.Genre)) dict["genres"] = request.Genre;
        if (!string.IsNullOrWhiteSpace(request.Platforms)) dict["platforms"] = request.Platforms;

        var startYear = request.StartYear.GetValueOrDefault(2010);
        var endYear = request.EndYear.GetValueOrDefault(DateTime.UtcNow.Year);
        dict["dates"] = $"{startYear:D4}-01-01,{endYear:D4}-12-31";

        return dict;
    }

    private async Task<DiscoveryRandomResponseDto> GetRandomUniformAsync(
        DiscoveryRandomRequestDto request,
        HashSet<int> excludedIds,
        DiscoveryMetrics metrics,
        CancellationToken cancellationToken)
    {
        var baseParams = BuildBaseParams(request);

        var count = await GetCountAsync(baseParams, metrics, cancellationToken);
        if (count <= 0)
        {
            return new DiscoveryRandomResponseDto
            {
                Success = false,
                Error = "No games found. Adjust your parameters.",
                IsUpstreamFailure = false
            };
        }

        var allResults = new List<(int Index, JsonElement Element)>();
        var anyUpstreamError = false;

        var randomOffset = Random.Shared.NextInt64(0, count);
        var targetPage = (int)(randomOffset / PageSize) + 1;
        var targetIndex = (int)(randomOffset % PageSize);

        var (primaryStatus, primaryBody) = await FetchPageAsync(baseParams, "", targetPage, PageSize, metrics, cancellationToken);
        if (IsUpstreamError(primaryStatus))
        {
            anyUpstreamError = true;
        }
        else if (primaryStatus is >= 200 and < 300)
        {
            ParseResultsIntoList(primaryBody, allResults, targetIndex);
        }

        if (allResults.Count == 0 && !anyUpstreamError)
        {
            var fallbackPages = GenerateFallbackPages(count, targetPage, MaxUniformFallbackPages);
            var fallbackTasks = fallbackPages.Select(p => FetchPageAsync(baseParams, "", p, PageSize, metrics, cancellationToken)).ToArray();
            var fallbackResults = await Task.WhenAll(fallbackTasks);

            foreach (var (status, body) in fallbackResults)
            {
                if (status is >= 200 and < 300)
                {
                    ParseResultsIntoList(body, allResults);
                }
                else if (IsUpstreamError(status))
                {
                    anyUpstreamError = true;
                }
            }
        }

        if (anyUpstreamError && allResults.Count == 0)
        {
            return new DiscoveryRandomResponseDto
            {
                Success = false,
                Error = "Game catalog temporarily unavailable.",
                IsUpstreamFailure = true
            };
        }

        var candidateElements = FilterExcluded(allResults.Select(r => r.Element).ToList(), excludedIds);

        if (candidateElements.Count == 0)
        {
            return new DiscoveryRandomResponseDto
            {
                Success = false,
                Error = allResults.Count > 0
                    ? "Exhausted unique results for these parameters."
                    : "No games found. Adjust your parameters.",
                IsUpstreamFailure = false
            };
        }

        for (var attempt = 0; attempt < MaxUniformRepicks; attempt++)
        {
            var chosen = candidateElements[Random.Shared.Next(0, candidateElements.Count)];
            if (!TryGetInt(chosen, "id", out var id) || excludedIds.Contains(id))
            {
                metrics.Repicks++;
                continue;
            }

            var game = JsonSerializer.Deserialize<JsonElement>(chosen.GetRawText());
            return new DiscoveryRandomResponseDto { Success = true, Game = game };
        }

        return new DiscoveryRandomResponseDto
        {
            Success = false,
            Error = "Exhausted unique results for these parameters.",
            IsUpstreamFailure = false
        };
    }

    private async Task<DiscoveryRandomResponseDto> GetRandomHighRatedAsync(
        DiscoveryRandomRequestDto request,
        HashSet<int> excludedIds,
        DiscoveryMetrics metrics,
        CancellationToken cancellationToken)
    {
        var minRating = request.MinRating.GetValueOrDefault(0m);
        var baseParams = BuildBaseParams(request);
        const string ordering = "-rating";

        var pool = new List<JsonElement>();
        var seen = new HashSet<int>();
        var anyAbove = false;
        var anyUpstreamError = false;

        for (var page = 1; page <= MaxHighRatingScanPages; page++)
        {
            var (status, body) = await FetchPageAsync(baseParams, ordering, page, PageSize, metrics, cancellationToken);

            if (IsUpstreamError(status))
            {
                anyUpstreamError = true;
                break;
            }

            if (status == StatusCodes.Status404NotFound) break;
            if (status is not (>= 200 and < 300)) break;

            using var doc = JsonDocument.Parse(body);
            if (!doc.RootElement.TryGetProperty("results", out var resultsEl) || resultsEl.ValueKind != JsonValueKind.Array)
                break;

            if (resultsEl.GetArrayLength() == 0) break;

            decimal? firstRated = null;
            foreach (var g in resultsEl.EnumerateArray())
            {
                if (!TryGetInt(g, "id", out var id)) continue;
                var rating = TryGetDecimal(g, "rating");
                if (!rating.HasValue) continue;

                firstRated ??= rating.Value;

                if (rating.Value >= minRating)
                {
                    anyAbove = true;
                    if (seen.Add(id))
                        pool.Add(g.Clone());
                }
            }

            if (pool.Count >= HighRatingPoolThreshold) break;
            if (firstRated.HasValue && firstRated.Value < minRating) break;
        }

        if (pool.Count < HighRatingPoolThreshold && !anyUpstreamError)
        {
            var scanStartPage = MaxHighRatingScanPages + 1;
            var fallbackTasks = Enumerable.Range(0, HighRatingFallbackPages)
                .Select(i => FetchPageAsync(baseParams, ordering, scanStartPage + i, PageSize, metrics, cancellationToken))
                .ToArray();
            var fallbackResults = await Task.WhenAll(fallbackTasks);

            foreach (var (status, body) in fallbackResults)
            {
                if (status is not (>= 200 and < 300)) continue;

                using var doc = JsonDocument.Parse(body);
                if (!doc.RootElement.TryGetProperty("results", out var resultsEl) || resultsEl.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (var g in resultsEl.EnumerateArray())
                {
                    if (!TryGetInt(g, "id", out var id)) continue;
                    var rating = TryGetDecimal(g, "rating");
                    if (!rating.HasValue || rating.Value < minRating) continue;

                    anyAbove = true;
                    if (seen.Add(id))
                        pool.Add(g.Clone());
                }
            }
        }

        var candidates = FilterExcluded(pool, excludedIds);

        if (candidates.Count == 0)
        {
            if (anyUpstreamError && pool.Count == 0)
            {
                return new DiscoveryRandomResponseDto
                {
                    Success = false,
                    Error = "Game catalog temporarily unavailable.",
                    IsUpstreamFailure = true
                };
            }

            return new DiscoveryRandomResponseDto
            {
                Success = false,
                Error = anyAbove
                    ? "Exhausted unique results for these parameters."
                    : "No games found. Adjust your parameters.",
                IsUpstreamFailure = false
            };
        }

        for (var i = 0; i < MaxHighRatingRepicks; i++)
        {
            var chosen = candidates[Random.Shared.Next(0, candidates.Count)];
            if (!TryGetInt(chosen, "id", out var id) || excludedIds.Contains(id))
            {
                metrics.Repicks++;
                continue;
            }

            var game = JsonSerializer.Deserialize<JsonElement>(chosen.GetRawText());
            return new DiscoveryRandomResponseDto { Success = true, Game = game };
        }

        return new DiscoveryRandomResponseDto
        {
            Success = false,
            Error = "Exhausted unique results for these parameters.",
            IsUpstreamFailure = false
        };
    }

    private async Task<long> GetCountAsync(
        Dictionary<string, string?> baseParams,
        DiscoveryMetrics metrics,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"discovery:count:{baseParams.GetHashCode()}";
        if (cache.TryGetValue<long>(cacheKey, out var cached))
        {
            return cached;
        }

        var (status, body) = await FetchPageAsync(baseParams, "", page: 1, pageSize: 1, metrics, cancellationToken);
        if (status is not (>= 200 and < 300))
        {
            return 0;
        }

        var count = ParseCountLong(body);
        if (count > 0)
        {
            cache.Set(cacheKey, count, CountCacheTtl);
        }

        return count;
    }

    private async Task<RawgPageResult> FetchPageAsync(
        Dictionary<string, string?> baseParams,
        string ordering,
        int page,
        int pageSize,
        DiscoveryMetrics metrics,
        CancellationToken cancellationToken)
    {
        metrics.RawgCalls += 1;

        var query = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
        foreach (var (k, v) in baseParams)
        {
            if (!string.IsNullOrWhiteSpace(v))
                query[k] = new StringValues(v);
        }

        if (!string.IsNullOrWhiteSpace(ordering))
            query["ordering"] = new StringValues(ordering);

        query["page"] = new StringValues(page.ToString());
        query["page_size"] = new StringValues(pageSize.ToString());

        var q = new QueryCollection(query);
        var (status, body) = await rawg.GetAsync("/games", q);
        return new RawgPageResult(status, body);
    }

    private void ParseResultsIntoList(string body, List<(int Index, JsonElement Element)> target, int? preferredIndex = null)
    {
        using var doc = JsonDocument.Parse(body);
        if (!doc.RootElement.TryGetProperty("results", out var resultsEl) || resultsEl.ValueKind != JsonValueKind.Array)
            return;

        var len = resultsEl.GetArrayLength();
        if (len == 0) return;

        if (preferredIndex.HasValue && preferredIndex.Value >= 0 && preferredIndex.Value < len)
        {
            target.Add((preferredIndex.Value, resultsEl[preferredIndex.Value].Clone()));
        }

        for (var i = 0; i < len; i++)
        {
            if (preferredIndex.HasValue && i == preferredIndex.Value) continue;
            target.Add((i, resultsEl[i].Clone()));
        }
    }

    private List<int> GenerateFallbackPages(long count, int targetPage, int numPages)
    {
        var maxPage = (int)Math.Min((count + PageSize - 1) / PageSize, int.MaxValue);
        var pages = new HashSet<int>();
        var rng = new Random();

        for (var i = 0; i < numPages * 3 && pages.Count < numPages; i++)
        {
            var offset = rng.Next(-5, 6);
            var candidate = targetPage + offset;
            if (candidate >= 1 && candidate <= maxPage && candidate != targetPage)
            {
                pages.Add(candidate);
            }
        }

        while (pages.Count < numPages && pages.Count < maxPage)
        {
            var candidate = rng.Next(1, maxPage + 1);
            pages.Add(candidate);
        }

        return pages.Take(numPages).ToList();
    }

    private List<JsonElement> FilterExcluded(List<JsonElement> elements, HashSet<int> excludedIds)
    {
        var result = new List<JsonElement>();
        foreach (var el in elements)
        {
            if (TryGetInt(el, "id", out var id) && !excludedIds.Contains(id))
            {
                result.Add(el);
            }
        }
        return result;
    }

    private static bool IsUpstreamError(int status) => status is >= 500 or 429 or 403 or 401;

    /// <summary>RAWG <c>count</c> can exceed <see cref="int.MaxValue"/>; <see cref="JsonElement.TryGetInt32"/> would fail and yield 0 games.</summary>
    private static long ParseCountLong(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (!doc.RootElement.TryGetProperty("count", out var countEl))
                return 0;

            if (countEl.TryGetInt64(out var l))
                return l;
            if (countEl.TryGetDouble(out var d) && d >= 0)
                return (long)d;
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    private static bool TryGetInt(JsonElement el, string property, out int value)
    {
        value = 0;
        return el.TryGetProperty(property, out var p) && p.TryGetInt32(out value);
    }

    private static decimal? TryGetDecimal(JsonElement el, string property)
    {
        if (!el.TryGetProperty(property, out var p)) return null;
        if (p.ValueKind == JsonValueKind.Number && p.TryGetDecimal(out var d)) return d;
        return null;
    }
}
