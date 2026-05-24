namespace videogame_randomized_back.Services.Discovery;

internal static class DiscoveryFallbackPages
{
    public static List<int> Generate(long count, int targetPage, int numPages, int pageSize)
    {
        var maxPage = (int)Math.Min((count + pageSize - 1) / pageSize, int.MaxValue);
        var pages = new HashSet<int>();
        var rng = Random.Shared;

        for (var i = 0; i < numPages * 3 && pages.Count < numPages; i++)
        {
            var offset = rng.Next(-5, 6);
            var candidate = targetPage + offset;
            if (candidate >= 1 && candidate <= maxPage && candidate != targetPage)
                pages.Add(candidate);
        }

        while (pages.Count < numPages && pages.Count < maxPage)
        {
            var candidate = rng.Next(1, maxPage + 1);
            pages.Add(candidate);
        }

        return pages.Take(numPages).ToList();
    }
}
