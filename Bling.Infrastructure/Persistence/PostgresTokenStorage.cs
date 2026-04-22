using System.Text.Json;
using Bling.Application.Interfaces;
using Npgsql;

namespace Bling.Infrastructure.Persistence;

/// <summary>
/// Armazenamento de tokens OAuth em PostgreSQL.
/// Implementa ITokenStorage (port da Application).
/// </summary>
public class PostgresTokenStorage : ITokenStorage
{
    private readonly string _connectionString;

    public PostgresTokenStorage(string connectionString)
    {
        _connectionString = connectionString;
        InitializeTable();
    }

    public async Task<bool> StoreTokensAsync(string accessToken, string refreshToken, DateTime expiresAt)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(@"
                INSERT INTO mcp_tokens (id, access_token, refresh_token, expires_at, updated_at)
                VALUES (1, @access, @refresh, @expires, @now)
                ON CONFLICT (id) DO UPDATE SET 
                    access_token = @access,
                    refresh_token = @refresh,
                    expires_at = @expires,
                    updated_at = @now", conn);

            cmd.Parameters.AddWithValue("access", accessToken);
            cmd.Parameters.AddWithValue("refresh", refreshToken);
            cmd.Parameters.AddWithValue("expires", expiresAt);
            cmd.Parameters.AddWithValue("now", DateTime.UtcNow);
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[TokenStorage] Store error: {ex.Message}");
            return false;
        }
    }

    public async Task<(string? accessToken, string? refreshToken, DateTime expiresAt)> RetrieveTokensAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "SELECT access_token, refresh_token, expires_at FROM mcp_tokens WHERE id = 1", conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            
            if (!await reader.ReadAsync())
                return (null, null, DateTime.MinValue);

            return (
                reader.GetString(0),
                reader.GetString(1),
                reader.GetDateTime(2)
            );
        }
        catch
        {
            return (null, null, DateTime.MinValue);
        }
    }

    public async Task<bool> ClearTokensAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand("DELETE FROM mcp_tokens WHERE id = 1", conn);
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch { return false; }
    }

    private void InitializeTable()
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS mcp_tokens (
                    id INTEGER PRIMARY KEY,
                    access_token TEXT NOT NULL,
                    refresh_token TEXT NOT NULL,
                    expires_at TIMESTAMP NOT NULL,
                    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
                );", conn);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { Console.Error.WriteLine($"[TokenStorage] Init error: {ex.Message}"); }
    }
}
