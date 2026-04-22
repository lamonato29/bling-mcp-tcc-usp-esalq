using System.Text.Json;
using Bling.Application.Interfaces;
using Npgsql;
using NpgsqlTypes;

namespace Bling.Infrastructure.Persistence;

/// <summary>
/// Implementação de cache persistente em PostgreSQL.
/// Implementa ICacheService (port da Application).
/// </summary>
public class PostgresCacheService : ICacheService
{
    private readonly string _connectionString;
    private readonly JsonSerializerOptions _jsonOptions;

    public PostgresCacheService(string connectionString)
    {
        _connectionString = connectionString;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        InitializeTable();
    }

    public int Count
    {
        get
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM mcp_cache", conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }

    public T? Get<T>(string key)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand("SELECT value FROM mcp_cache WHERE key = @key", conn);
            cmd.Parameters.AddWithValue("key", key);
            var result = cmd.ExecuteScalar() as string;
            return result != null ? JsonSerializer.Deserialize<T>(result, _jsonOptions) : default;
        }
        catch { return default; }
    }

    public T? GetIfNotExpired<T>(string key)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(
                "SELECT value FROM mcp_cache WHERE key = @key AND (expires_at IS NULL OR expires_at > @now)", conn);
            cmd.Parameters.AddWithValue("key", key);
            cmd.Parameters.AddWithValue("now", DateTime.UtcNow);
            var result = cmd.ExecuteScalar() as string;
            return result != null ? JsonSerializer.Deserialize<T>(result, _jsonOptions) : default;
        }
        catch { return default; }
    }

    public void Set<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(@"
                INSERT INTO mcp_cache (key, value, created_at, expires_at) 
                VALUES (@key, @value, @now, NULL)
                ON CONFLICT (key) DO UPDATE SET value = @value, created_at = @now, expires_at = NULL", conn);
            cmd.Parameters.AddWithValue("key", key);
            cmd.Parameters.Add(new NpgsqlParameter("value", NpgsqlDbType.Jsonb) { Value = json });
            cmd.Parameters.AddWithValue("now", DateTime.UtcNow);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { Console.Error.WriteLine($"[Cache] Set error: {ex.Message}"); }
    }

    public void SetWithTtl<T>(string key, T value, TimeSpan ttl)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            var expiresAt = DateTime.UtcNow + ttl;
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(@"
                INSERT INTO mcp_cache (key, value, created_at, expires_at) 
                VALUES (@key, @value, @now, @expires)
                ON CONFLICT (key) DO UPDATE SET value = @value, created_at = @now, expires_at = @expires", conn);
            cmd.Parameters.AddWithValue("key", key);
            cmd.Parameters.Add(new NpgsqlParameter("value", NpgsqlDbType.Jsonb) { Value = json });
            cmd.Parameters.AddWithValue("now", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("expires", expiresAt);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { Console.Error.WriteLine($"[Cache] SetWithTtl error: {ex.Message}"); }
    }

    public void Remove(string key)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand("DELETE FROM mcp_cache WHERE key = @key", conn);
            cmd.Parameters.AddWithValue("key", key);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { Console.Error.WriteLine($"[Cache] Remove error: {ex.Message}"); }
    }

    public void RemoveByPrefix(string prefix)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand("DELETE FROM mcp_cache WHERE key LIKE @prefix", conn);
            cmd.Parameters.AddWithValue("prefix", prefix + "%");
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { Console.Error.WriteLine($"[Cache] RemoveByPrefix error: {ex.Message}"); }
    }

    public IEnumerable<string> GetKeys()
    {
        var keys = new List<string>();
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand("SELECT key FROM mcp_cache", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                keys.Add(reader.GetString(0));
        }
        catch { }
        return keys;
    }

    private void InitializeTable()
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS mcp_cache (
                    key TEXT PRIMARY KEY,
                    value JSONB NOT NULL,
                    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
                    expires_at TIMESTAMP NULL
                );
                CREATE INDEX IF NOT EXISTS idx_mcp_cache_expires ON mcp_cache (expires_at);", conn);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { Console.Error.WriteLine($"[Cache] Init error: {ex.Message}"); }
    }
}
