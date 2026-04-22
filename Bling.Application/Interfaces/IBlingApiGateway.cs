using System.Text.Json.Nodes;

namespace Bling.Application.Interfaces;

/// <summary>
/// Port para o cliente HTTP da API Bling v3.
/// Abstrai todas as chamadas HTTP para a API externa.
/// </summary>
public interface IBlingApiGateway
{
    /// <summary>
    /// Executa GET na API do Bling
    /// </summary>
    Task<JsonObject?> GetAsync(string endpoint, Dictionary<string, string>? queryParams = null);
    
    /// <summary>
    /// Executa POST na API do Bling
    /// </summary>
    Task<JsonObject?> PostAsync(string endpoint, JsonObject body);
    
    /// <summary>
    /// Executa PUT na API do Bling
    /// </summary>
    Task<JsonObject?> PutAsync(string endpoint, JsonObject body);
    
    /// <summary>
    /// Executa DELETE na API do Bling
    /// </summary>
    Task<bool> DeleteAsync(string endpoint);
    
    /// <summary>
    /// Executa PATCH na API do Bling
    /// </summary>
    Task<JsonObject?> PatchAsync(string endpoint, JsonObject? body = null);
}
