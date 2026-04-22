using System.Text.Json;
using Bling.Application.Interfaces;

namespace Bling.Infrastructure.Persistence;

/// <summary>
/// Armazenamento de tokens OAuth em arquivo JSON.
/// Implementa ITokenStorage (port da Application).
/// </summary>
public class FileTokenStorage : ITokenStorage
{
    private readonly string _filePath;

    public FileTokenStorage(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<bool> StoreTokensAsync(string accessToken, string refreshToken, DateTime expiresAt)
    {
        try
        {
            var data = new TokenFileData
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt
            };

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            await File.WriteAllTextAsync(_filePath, json);
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
            if (!File.Exists(_filePath))
                return (null, null, DateTime.MinValue);

            var json = await File.ReadAllTextAsync(_filePath);
            var data = JsonSerializer.Deserialize<TokenFileData>(json);
            
            if (data == null)
                return (null, null, DateTime.MinValue);

            return (data.AccessToken, data.RefreshToken, data.ExpiresAt);
        }
        catch
        {
            return (null, null, DateTime.MinValue);
        }
    }

    public Task<bool> ClearTokensAsync()
    {
        try
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private class TokenFileData
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
