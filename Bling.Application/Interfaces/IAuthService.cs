namespace Bling.Application.Interfaces;

/// <summary>
/// Port para armazenamento seguro de tokens OAuth
/// </summary>
public interface ITokenStorage
{
    /// <summary>
    /// Armazena os tokens OAuth
    /// </summary>
    Task<bool> StoreTokensAsync(string accessToken, string refreshToken, DateTime expiresAt);
    
    /// <summary>
    /// Recupera os tokens armazenados
    /// </summary>
    Task<(string? accessToken, string? refreshToken, DateTime expiresAt)> RetrieveTokensAsync();
    
    /// <summary>
    /// Remove todos os tokens armazenados
    /// </summary>
    Task<bool> ClearTokensAsync();
}

/// <summary>
/// Port para serviço de autenticação OAuth
/// </summary>
public interface IAuthService
{
    string? AccessToken { get; }
    string? RefreshToken { get; }
    DateTime TokenExpiresAt { get; }
    
    bool IsTokenValid();
    Task<bool> AuthorizeAsync();
    Task<bool> RefreshTokenAsync();
    Task<bool> EnsureValidTokenAsync();
    void SetTokens(string accessToken, string refreshToken, DateTime expiresAt);
    void ClearTokens();
    Task LoadTokensAsync();
}
