namespace F1_MlFlow.Services.State;

public sealed class UiCacheService : IUiCacheService
{
    private readonly Dictionary<string, CacheEntry> _cache = new(StringComparer.OrdinalIgnoreCase);

    public bool TryGet<T>(string key, out T? value, out int? latencyMs, out DateTimeOffset cachedAt)
    {
        value = default;
        latencyMs = null;
        cachedAt = default;

        if (!_cache.TryGetValue(key, out var entry))
        {
            return false;
        }

        if (entry.Value is T typed)
        {
            value = typed;
            latencyMs = entry.LatencyMs;
            cachedAt = entry.CachedAt;
            return true;
        }

        return false;
    }

    public void Set<T>(string key, T value, int? latencyMs, DateTimeOffset cachedAt)
    {
        _cache[key] = new CacheEntry(value!, latencyMs, cachedAt);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }

    private sealed record CacheEntry(object Value, int? LatencyMs, DateTimeOffset CachedAt);
}
