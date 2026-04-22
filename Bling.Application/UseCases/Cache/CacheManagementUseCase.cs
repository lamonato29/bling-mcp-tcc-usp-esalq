using Bling.Application.DTOs;
using Bling.Application.Interfaces;

namespace Bling.Application.UseCases.Cache;

/// <summary>
/// Use Case: Gerenciar cache (refresh, invalidação)
/// </summary>
public class CacheManagementUseCase
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly ICacheService _cache;

    public CacheManagementUseCase(IPedidoRepository pedidoRepository, ICacheService cache)
    {
        _pedidoRepository = pedidoRepository;
        _cache = cache;
    }

    /// <summary>
    /// Refresh de um pedido específico (busca novamente e atualiza cache)
    /// </summary>
    public async Task<bool> RefreshPedidoAsync(long id)
    {
        _cache.Remove($"pedido:{id}");
        var pedido = await _pedidoRepository.ObterPorIdAsync(id);
        return pedido != null;
    }

    /// <summary>
    /// Refresh de múltiplos pedidos
    /// </summary>
    public async Task<int> RefreshPedidosBatchAsync(IEnumerable<long> ids)
    {
        var count = 0;
        foreach (var id in ids)
        {
            if (await RefreshPedidoAsync(id)) count++;
            await Task.Delay(100);
        }
        return count;
    }

    /// <summary>
    /// Invalidar todo cache de pedidos
    /// </summary>
    public void InvalidarPedidos() => _cache.RemoveByPrefix("pedido:");

    /// <summary>
    /// Invalidar cache de produtos
    /// </summary>
    public void InvalidarProdutos() => _cache.RemoveByPrefix("produto:");

    /// <summary>
    /// Invalidar cache de canais de venda
    /// </summary>
    public void InvalidarCanais() => _cache.RemoveByPrefix("canal:");

    /// <summary>
    /// Invalidar cache de situações
    /// </summary>
    public void InvalidarSituacoes() => _cache.RemoveByPrefix("situacao");

    /// <summary>
    /// Invalidar cache de depósitos
    /// </summary>
    public void InvalidarDepositos() => _cache.RemoveByPrefix("deposito:");

    /// <summary>
    /// Invalidar todo o cache
    /// </summary>
    public int InvalidarTudo()
    {
        var keys = _cache.GetKeys().ToList();
        foreach (var key in keys) _cache.Remove(key);
        return keys.Count;
    }

    /// <summary>
    /// Obter estatísticas do cache
    /// </summary>
    public CacheStatsDto GetStats()
    {
        var keys = _cache.GetKeys().ToList();
        return new CacheStatsDto
        {
            TotalItens = _cache.Count,
            Pedidos = keys.Count(k => k.StartsWith("pedido:")),
            Produtos = keys.Count(k => k.StartsWith("produto:")),
            Canais = keys.Count(k => k.StartsWith("canal:")),
            Situacoes = keys.Count(k => k.StartsWith("situacao")),
            Depositos = keys.Count(k => k.StartsWith("deposito:")),
            Outros = keys.Count(k => !k.StartsWith("pedido:") && !k.StartsWith("produto:")
                && !k.StartsWith("canal:") && !k.StartsWith("situacao") && !k.StartsWith("deposito:"))
        };
    }
}
