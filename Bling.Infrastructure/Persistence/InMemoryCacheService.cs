using System.Collections.Concurrent;
using Bling.Application.Interfaces;

namespace Bling.Infrastructure.Persistence;

/// <summary>
/// Cache em memória local (fallback quando PostgreSQL não está disponível)
/// </summary>
public class InMemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

    public int Count => _cache.Count;

    public T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out var entry))
            return entry.GetValue<T>();
        return default;
    }

    public T? GetIfNotExpired<T>(string key)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt.HasValue && entry.ExpiresAt.Value < DateTime.UtcNow)
            {
                _cache.TryRemove(key, out _);
                return default;
            }
            return entry.GetValue<T>();
        }
        return default;
    }

    public void Set<T>(string key, T value)
    {
        _cache[key] = new CacheEntry(value!, null);
    }

    public void SetWithTtl<T>(string key, T value, TimeSpan ttl)
    {
        _cache[key] = new CacheEntry(value!, DateTime.UtcNow + ttl);
    }

    public void Remove(string key)
    {
        _cache.TryRemove(key, out _);
    }

    public void RemoveByPrefix(string prefix)
    {
        foreach (var key in _cache.Keys)
        {
            if (key.StartsWith(prefix))
                _cache.TryRemove(key, out _);
        }
    }

    public IEnumerable<string> GetKeys() => _cache.Keys;

    private class CacheEntry
    {
        public object Value { get; }
        public DateTime? ExpiresAt { get; }

        public CacheEntry(object value, DateTime? expiresAt)
        {
            Value = value;
            ExpiresAt = expiresAt;
        }

        public T? GetValue<T>()
        {
            if (Value is T typed) return typed;
            return default;
        }
    }
}
