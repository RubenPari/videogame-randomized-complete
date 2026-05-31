using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using videogame_randomized_back.Data;
using videogame_randomized_back.DTOs;
using videogame_randomized_back.Services.Discovery;

namespace videogame_randomized_back.Services;

public class DiscoveryService(
    ILogger<DiscoveryService> logger,
    AppDbContext db,
    IRawgService rawg,
    IMemoryCache cache) : IDiscoveryService
{
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

    private async Task<DiscoveryRandomResponseDto> GetRandomUniformAsync(
        DiscoveryRandomRequestDto request,
        HashSet<int> excludedIds,
        DiscoveryMetrics metrics,
        CancellationToken cancellationToken)
    {
        var baseParams = DiscoveryQueryParameters.BuildBase(request);

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

        // Cap to RAWG's accessible range to avoid requesting pages that return 404
        var effectiveCount = Math.Min(count, DiscoveryConstants.MaxAccessibleResults);

        var allResults = new List<(int Index, JsonElement Element)>();
        var anyUpstreamError = false;

        var randomOffset = Random.Shared.NextInt64(0, effectiveCount);
        var targetPage = (int)(randomOffset / DiscoveryConstants.PageSize) + 1;
        var targetIndex = (int)(randomOffset % DiscoveryConstants.PageSize);

        var (primaryStatus, primaryBody) = await FetchPageAsync(
            baseParams, "", targetPage, DiscoveryConstants.PageSize, metrics, cancellationToken);
        if (RawgDiscoveryJson.IsUpstreamError(primaryStatus))
        {
            anyUpstreamError = true;
        }
        else if (primaryStatus is >= 200 and < 300)
        {
            RawgDiscoveryJson.AppendResultsToList(primaryBody, allResults, targetIndex);
        }

        if (allResults.Count == 0 && !anyUpstreamError)
        {
            var fallbackPages = DiscoveryFallbackPages.Generate(
                effectiveCount,
                targetPage,
                DiscoveryConstants.MaxUniformFallbackPages,
                DiscoveryConstants.PageSize);
            var fallbackTasks = fallbackPages
                .Select(p => FetchPageAsync(baseParams, "", p, DiscoveryConstants.PageSize, metrics, cancellationToken))
                .ToArray();
            var fallbackResults = await Task.WhenAll(fallbackTasks);

            foreach (var (status, body) in fallbackResults)
            {
                if (status is >= 200 and < 300)
                    RawgDiscoveryJson.AppendResultsToList(body, allResults);
                else if (RawgDiscoveryJson.IsUpstreamError(status))
                    anyUpstreamError = true;
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

        var candidateElements = RawgDiscoveryJson.FilterExcluded(
            allResults.Select(r => r.Element).ToList(),
            excludedIds);

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

        var repicks = metrics.Repicks;
        var picked = DiscoveryRandomPicker.TryPickWithRepicks(
            candidateElements,
            excludedIds,
            DiscoveryConstants.MaxUniformRepicks,
            ref repicks);
        metrics.Repicks = repicks;
        if (picked != null)
            return picked;

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
        var baseParams = DiscoveryQueryParameters.BuildBase(request);
        const string ordering = "-rating";

        var pool = new List<JsonElement>();
        var seen = new HashSet<int>();
        var anyAbove = false;
        var anyUpstreamError = false;

        for (var page = 1; page <= DiscoveryConstants.MaxHighRatingScanPages; page++)
        {
            var (status, body) = await FetchPageAsync(
                baseParams, ordering, page, DiscoveryConstants.PageSize, metrics, cancellationToken);

            if (RawgDiscoveryJson.IsUpstreamError(status))
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

            HighRatingPageProcessor.CollectFromScanPage(
                resultsEl,
                minRating,
                seen,
                pool,
                ref anyAbove,
                out var firstRated);

            if (pool.Count >= DiscoveryConstants.HighRatingPoolThreshold) break;
            if (firstRated.HasValue && firstRated.Value < minRating) break;
        }

        if (pool.Count < DiscoveryConstants.HighRatingPoolThreshold && !anyUpstreamError)
        {
            var scanStartPage = DiscoveryConstants.MaxHighRatingScanPages + 1;
            var fallbackTasks = Enumerable.Range(0, DiscoveryConstants.HighRatingFallbackPages)
                .Select(i => FetchPageAsync(
                    baseParams,
                    ordering,
                    scanStartPage + i,
                    DiscoveryConstants.PageSize,
                    metrics,
                    cancellationToken))
                .ToArray();
            var fallbackResults = await Task.WhenAll(fallbackTasks);

            foreach (var (status, body) in fallbackResults)
            {
                if (status is not (>= 200 and < 300)) continue;

                using var doc = JsonDocument.Parse(body);
                if (!doc.RootElement.TryGetProperty("results", out var resultsEl) ||
                    resultsEl.ValueKind != JsonValueKind.Array)
                    continue;

                HighRatingPageProcessor.CollectFromFallbackPage(
                    resultsEl,
                    minRating,
                    seen,
                    pool,
                    ref anyAbove);
            }
        }

        var candidates = RawgDiscoveryJson.FilterExcluded(pool, excludedIds);

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

        var repicksHr = metrics.Repicks;
        var pickedHr = DiscoveryRandomPicker.TryPickWithRepicks(
            candidates,
            excludedIds,
            DiscoveryConstants.MaxHighRatingRepicks,
            ref repicksHr);
        metrics.Repicks = repicksHr;
        if (pickedHr != null)
            return pickedHr;

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
        var sortedParams = baseParams
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .OrderBy(kv => kv.Key)
            .Select(kv => $"{kv.Key}={kv.Value}");
        var cacheKey = $"discovery:count:{string.Join("&", sortedParams)}";
        if (cache.TryGetValue<long>(cacheKey, out var cached))
            return cached;

        var (status, body) = await FetchPageAsync(baseParams, "", page: 1, pageSize: 1, metrics, cancellationToken);
        if (status is not (>= 200 and < 300))
            return 0;

        var count = RawgDiscoveryJson.ParseCountLong(body);
        if (count > 0)
            cache.Set(cacheKey, count, DiscoveryConstants.CountCacheTtl);

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
}
