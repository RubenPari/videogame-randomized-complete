using System.Text.Json;

namespace videogame_randomized_back.Services.Discovery;

internal static class RawgDiscoveryJson
{
    public static bool IsUpstreamError(int status) => status is >= 500 or 429 or 403 or 401;

    /// <summary>
    /// RAWG <c>count</c> can exceed <see cref="int.MaxValue"/>; <see cref="JsonElement.TryGetInt32"/> would fail and yield 0 games.
    /// </summary>
    public static long ParseCountLong(string body)
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

    public static void AppendResultsToList(
        string body,
        List<(int Index, JsonElement Element)> target,
        int? preferredIndex = null)
    {
        using var doc = JsonDocument.Parse(body);
        if (!doc.RootElement.TryGetProperty("results", out var resultsEl) || resultsEl.ValueKind != JsonValueKind.Array)
            return;

        var len = resultsEl.GetArrayLength();
        if (len == 0) return;

        if (preferredIndex.HasValue && preferredIndex.Value >= 0 && preferredIndex.Value < len)
            target.Add((preferredIndex.Value, resultsEl[preferredIndex.Value].Clone()));

        for (var i = 0; i < len; i++)
        {
            if (preferredIndex.HasValue && i == preferredIndex.Value) continue;
            target.Add((i, resultsEl[i].Clone()));
        }
    }

    public static List<JsonElement> FilterExcluded(List<JsonElement> elements, HashSet<int> excludedIds)
    {
        var result = new List<JsonElement>();
        foreach (var el in elements)
        {
            if (TryGetInt(el, "id", out var id) && !excludedIds.Contains(id))
                result.Add(el);
        }
        return result;
    }

    public static bool TryGetInt(JsonElement el, string property, out int value)
    {
        value = 0;
        return el.TryGetProperty(property, out var p) && p.TryGetInt32(out value);
    }

    public static decimal? TryGetDecimal(JsonElement el, string property)
    {
        if (!el.TryGetProperty(property, out var p)) return null;
        if (p.ValueKind == JsonValueKind.Number && p.TryGetDecimal(out var d)) return d;
        return null;
    }
}
