using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using videogame_randomized_back.Data;
using videogame_randomized_back.DTOs;

namespace videogame_randomized_back.Services;

public class DiscoveryService(
    ILogger<DiscoveryService> logger,
    AppDbContext db,
    RawgService rawg,
    IMemoryCache cache)
{
    private const int DefaultPageSize = 40;
    private static readonly TimeSpan MaxValidPageTtl = TimeSpan.FromMinutes(20);

    private record MaxPageCacheEntry(long Count, int PageSize, int MaxValidPage, DateTimeOffset CachedAt);

    private sealed class DiscoveryMetrics
    {
        public int RawgCalls { get; set; }
        public int Repicks { get; set; }
    }

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

        var pageSize = DefaultPageSize;
        var minRating = request.MinRating.GetValueOrDefault(0m);
        var isHighRatingMode = minRating > 0m;
        var metrics = new DiscoveryMetrics();

        var excludedIds = await GetExcludedIdsAsync(userId, request.ExcludeIds, cancellationToken);

        if (isHighRatingMode)
        {
            var res = await GetRandomHighRatedAsync(request, excludedIds, pageSize, metrics, cancellationToken);
            logger.LogInformation(
                "Discovery(highRating) rawgCalls={RawgCalls} repicks={Repicks} excludedCount={ExcludedCount}",
                metrics.RawgCalls,
                metrics.Repicks,
                excludedIds.Count);
            return res;
        }

        var result = await GetRandomUniformAsync(request, excludedIds, pageSize, metrics, cancellationToken);
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

    private static string BuildCacheKey(DiscoveryRandomRequestDto request, string ordering)
    {
        var genre = request.Genre?.Trim() ?? "";
        var platforms = request.Platforms?.Trim() ?? "";
        var startYear = request.StartYear?.ToString() ?? "";
        var endYear = request.EndYear?.ToString() ?? "";
        var minRating = request.MinRating?.ToString() ?? "0";
        return $"discovery:maxValidPage:{genre}|{platforms}|{startYear}|{endYear}|{minRating}|{ordering}";
    }

    private async Task<DiscoveryRandomResponseDto> GetRandomUniformAsync(
        DiscoveryRandomRequestDto request,
        HashSet<int> excludedIds,
        int pageSize,
        DiscoveryMetrics metrics,
        CancellationToken cancellationToken)
    {
        const int maxRepicks = 5;
        var baseParams = BuildBaseParams(request);
        var ordering = "";
        var cacheKey = BuildCacheKey(request, ordering);

        var (count, maxValidPage, upstreamFailed) = await GetOrComputeMaxValidPageAsync(
            cacheKey,
            baseParams,
            ordering,
            pageSize,
            metrics,
            cancellationToken);

        if (upstreamFailed)
        {
            return new DiscoveryRandomResponseDto
            {
                Success = false,
                Error = "Game catalog temporarily unavailable.",
                IsUpstreamFailure = true
            };
        }

        if (count <= 0)
        {
            return new DiscoveryRandomResponseDto
            {
                Success = false,
                Error = "No games found. Adjust your parameters.",
                IsUpstreamFailure = false
            };
        }

        for (var attempt = 0; attempt < maxRepicks; attempt++)
        {
            var offset = Random.Shared.NextInt64(0, count);
            var page = (int)(offset / pageSize) + 1;
            var index = (int)(offset % pageSize);

            if (page > maxValidPage) page = maxValidPage;
            if (page < 1) page = 1;

            var (status, body, updatedMaxValidPage) = await TryFetchPageAndPickAsync(
                baseParams,
                ordering,
                page,
                pageSize,
                index,
                excludedIds,
                metrics,
                cancellationToken);

            if (updatedMaxValidPage.HasValue && updatedMaxValidPage.Value < maxValidPage)
            {
                maxValidPage = updatedMaxValidPage.Value;
                cache.Set(cacheKey, new MaxPageCacheEntry(count, pageSize, maxValidPage, DateTimeOffset.UtcNow), MaxValidPageTtl);
            }

            if (status is >= 200 and < 300)
            {
                var game = JsonSerializer.Deserialize<JsonElement>(body);
                return new DiscoveryRandomResponseDto { Success = true, Game = game };
            }

            if (status == StatusCodes.Status404NotFound)
            {
                metrics.Repicks += 1;
                continue;
            }

            if (status < 200 || status >= 300)
            {
                return new DiscoveryRandomResponseDto
                {
                    Success = false,
                    Error = "Game catalog temporarily unavailable.",
                    IsUpstreamFailure = status is >= 500 or 429 or 403 or 401
                };
            }
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
        int pageSize,
        DiscoveryMetrics metrics,
        CancellationToken cancellationToken)
    {
        var minRating = request.MinRating.GetValueOrDefault(0m);
        const int maxScanPages = 50;
        const int maxRepicks = 5;

        var baseParams = BuildBaseParams(request);
        var ordering = "-rating";

        var pool = new List<JsonElement>();
        var seen = new HashSet<int>();
        var anyAbove = false;

        for (var page = 1; page <= maxScanPages; page++)
        {
            var (status, body) = await RawgGetGamesAsync(baseParams, ordering, page, pageSize, metrics, cancellationToken);
            if (status == StatusCodes.Status404NotFound) break;
            if (status < 200 || status >= 300)
            {
                return new DiscoveryRandomResponseDto
                {
                    Success = false,
                    Error = "Game catalog temporarily unavailable.",
                    IsUpstreamFailure = status is >= 500 or 429 or 403 or 401
                };
            }

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
                        pool.Add(g);
                }
            }

            if (firstRated.HasValue && firstRated.Value < minRating) break;
        }

        var candidates = pool
            .Where(g => TryGetInt(g, "id", out var id) && !excludedIds.Contains(id))
            .ToList();

        if (candidates.Count == 0)
        {
            return new DiscoveryRandomResponseDto
            {
                Success = false,
                Error = anyAbove
                    ? "Exhausted unique results for these parameters."
                    : "No games found. Adjust your parameters.",
                IsUpstreamFailure = false
            };
        }

        for (var i = 0; i < maxRepicks; i++)
        {
            var chosen = candidates[Random.Shared.Next(0, candidates.Count)];
            if (!TryGetInt(chosen, "id", out var id) || excludedIds.Contains(id)) continue;

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

    private async Task<(long Count, int MaxValidPage, bool UpstreamFailed)> GetOrComputeMaxValidPageAsync(
        string cacheKey,
        Dictionary<string, string?> baseParams,
        string ordering,
        int pageSize,
        DiscoveryMetrics metrics,
        CancellationToken cancellationToken)
    {
        if (cache.TryGetValue<MaxPageCacheEntry>(cacheKey, out var cached) && cached is not null && cached.PageSize == pageSize)
        {
            logger.LogDebug("Discovery cache hit for {Key}: maxValidPage={MaxValidPage}, count={Count}", cacheKey, cached.MaxValidPage, cached.Count);
            return (cached.Count, cached.MaxValidPage, false);
        }

        logger.LogDebug("Discovery cache miss for {Key}", cacheKey);

        var (countStatus, countBody) = await RawgGetGamesAsync(baseParams, ordering, page: 1, pageSize: 1, metrics, cancellationToken);
        if (countStatus < 200 || countStatus >= 300)
            return (0, 1, true);

        var count = ParseCountLong(countBody);
        if (count <= 0)
        {
            cache.Set(cacheKey, new MaxPageCacheEntry(0, pageSize, 1, DateTimeOffset.UtcNow), MaxValidPageTtl);
            return (0, 1, false);
        }

        var maxPageLong = Math.Max(1L, (count + pageSize - 1) / pageSize);
        var hi = (int)Math.Min(maxPageLong, int.MaxValue);
        var maxValidPage = await FindMaxValidPageAsync(baseParams, ordering, hi, metrics, cancellationToken);

        cache.Set(cacheKey, new MaxPageCacheEntry(count, pageSize, maxValidPage, DateTimeOffset.UtcNow), MaxValidPageTtl);
        return (count, maxValidPage, false);
    }

    private async Task<int> FindMaxValidPageAsync(
        Dictionary<string, string?> baseParams,
        string ordering,
        int upperBound,
        DiscoveryMetrics metrics,
        CancellationToken cancellationToken)
    {
        var lo = 1;
        var hi = upperBound;
        var best = 1;

        for (var i = 0; i < 14 && lo <= hi; i++)
        {
            var mid = (lo + hi) / 2;
            var (status, _) = await RawgGetGamesAsync(baseParams, ordering, page: mid, pageSize: 1, metrics, cancellationToken);
            if (status is >= 200 and < 300)
            {
                best = mid;
                lo = mid + 1;
            }
            else if (status == StatusCodes.Status404NotFound)
            {
                hi = mid - 1;
            }
            else
            {
                break;
            }
        }

        return Math.Max(1, best);
    }

    private async Task<(int StatusCode, string Body, int? UpdatedMaxValidPage)> TryFetchPageAndPickAsync(
        Dictionary<string, string?> baseParams,
        string ordering,
        int page,
        int pageSize,
        int index,
        HashSet<int> excludedIds,
        DiscoveryMetrics metrics,
        CancellationToken cancellationToken)
    {
        var (status, body) = await RawgGetGamesAsync(baseParams, ordering, page, pageSize, metrics, cancellationToken);
        if (status == StatusCodes.Status404NotFound)
        {
            logger.LogDebug("RAWG returned 404 for page {Page}; will shrink maxValidPage to {NewMax}", page, Math.Max(1, page - 1));
            return (StatusCodes.Status404NotFound, body, Math.Max(1, page - 1));
        }

        if (status < 200 || status >= 300)
            return (status, body, null);

        using var doc = JsonDocument.Parse(body);
        if (!doc.RootElement.TryGetProperty("results", out var resultsEl) || resultsEl.ValueKind != JsonValueKind.Array)
            return (StatusCodes.Status404NotFound, """{"error":"No results."}""", null);

        var len = resultsEl.GetArrayLength();
        if (len == 0)
            return (StatusCodes.Status404NotFound, """{"error":"No results."}""", null);

        var chosen = index < len ? resultsEl[index] : resultsEl[Random.Shared.Next(0, len)];

        if (TryGetInt(chosen, "id", out var id) && excludedIds.Contains(id))
            return (StatusCodes.Status404NotFound, """{"error":"Excluded."}""", null);

        return (StatusCodes.Status200OK, chosen.GetRawText(), null);
    }

    private async Task<(int StatusCode, string Body)> RawgGetGamesAsync(
        Dictionary<string, string?> baseParams,
        string ordering,
        int page,
        int pageSize,
        DiscoveryMetrics? metrics,
        CancellationToken cancellationToken)
    {
        metrics?.RawgCalls += 1;

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
        return await rawg.GetAsync("/games", q);
    }

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
