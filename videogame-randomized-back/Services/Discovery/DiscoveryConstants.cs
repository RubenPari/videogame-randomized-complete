namespace videogame_randomized_back.Services.Discovery;

internal static class DiscoveryConstants
{
    public const int PageSize = 40;
    public const int MaxUniformFallbackPages = 3;
    public const int MaxHighRatingScanPages = 10;
    public const int HighRatingPoolThreshold = 100;
    public const int HighRatingFallbackPages = 5;
    public const int MaxUniformRepicks = 5;
    public const int MaxHighRatingRepicks = 5;

    /// <summary>
    /// RAWG API limits pagination; pages beyond this offset return 404.
    /// </summary>
    public const long MaxAccessibleResults = 10_000;

    public static readonly TimeSpan CountCacheTtl = TimeSpan.FromMinutes(5);
}
