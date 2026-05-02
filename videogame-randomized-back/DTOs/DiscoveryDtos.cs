using System.Text.Json;

namespace videogame_randomized_back.DTOs;

public record DiscoveryRandomRequestDto(
    string? Genre,
    string? Platforms,
    int? StartYear,
    int? EndYear,
    decimal? MinRating,
    string? ExcludeIds);

/// <summary>
/// Discovery random response. Use HTTP 200 for business outcomes so clients (Axios) can read
/// <see cref="Error"/> without treating it as a transport failure.
/// </summary>
public sealed class DiscoveryRandomResponseDto
{
    public bool Success { get; init; }
    public JsonElement? Game { get; init; }
    public string? Error { get; init; }

    /// <summary>When true, the controller should return 502 Bad Gateway instead of 200.</summary>
    public bool IsUpstreamFailure { get; init; }
}

