using System.Text.Json;
using videogame_randomized_back.DTOs;

namespace videogame_randomized_back.Services.Discovery;

internal static class DiscoveryRandomPicker
{
    public static DiscoveryRandomResponseDto? TryPickWithRepicks(
        List<JsonElement> candidateElements,
        HashSet<int> excludedIds,
        int maxRepicks,
        ref int repicks)
    {
        for (var attempt = 0; attempt < maxRepicks; attempt++)
        {
            var chosen = candidateElements[Random.Shared.Next(0, candidateElements.Count)];
            if (!RawgDiscoveryJson.TryGetInt(chosen, "id", out var id) || excludedIds.Contains(id))
            {
                repicks++;
                continue;
            }

            var game = JsonSerializer.Deserialize<JsonElement>(chosen.GetRawText());
            return new DiscoveryRandomResponseDto { Success = true, Game = game };
        }

        return null;
    }
}
