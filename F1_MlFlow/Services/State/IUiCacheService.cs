namespace F1_MlFlow.Services.State;

public interface IUiCacheService
{
    bool TryGet<T>(string key, out T? value, out int? latencyMs, out DateTimeOffset cachedAt);
    void Set<T>(string key, T value, int? latencyMs, DateTimeOffset cachedAt);
    void Remove(string key);
}
