namespace Bling.Application.Interfaces;

/// <summary>
/// Port para serviço de cache genérico.
/// Abstrai a implementação de cache (PostgreSQL, Redis, in-memory, etc.)
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Obtém um valor do cache
    /// </summary>
    T? Get<T>(string key);
    
    /// <summary>
    /// Obtém um valor do cache, retornando null se expirado
    /// </summary>
    T? GetIfNotExpired<T>(string key);
    
    /// <summary>
    /// Armazena um valor no cache (eternamente até refresh)
    /// </summary>
    void Set<T>(string key, T value);
    
    /// <summary>
    /// Armazena um valor no cache com TTL (tempo de vida)
    /// </summary>
    void SetWithTtl<T>(string key, T value, TimeSpan ttl);
    
    /// <summary>
    /// Remove uma chave específica do cache
    /// </summary>
    void Remove(string key);
    
    /// <summary>
    /// Remove todas as chaves que começam com o prefixo
    /// </summary>
    void RemoveByPrefix(string prefix);
    
    /// <summary>
    /// Obtém todas as chaves do cache
    /// </summary>
    IEnumerable<string> GetKeys();
    
    /// <summary>
    /// Quantidade de itens no cache
    /// </summary>
    int Count { get; }
}
