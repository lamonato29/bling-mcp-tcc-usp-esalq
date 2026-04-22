using Bling.Application.Interfaces;
using Bling.Domain.Entities;

namespace Bling.Application.UseCases.Pedidos;

/// <summary>
/// Use Case: Obter detalhes completos de um pedido por ID ou número
/// </summary>
public class ObterPedidoUseCase
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly ICacheService _cache;

    public ObterPedidoUseCase(IPedidoRepository pedidoRepository, ICacheService cache)
    {
        _pedidoRepository = pedidoRepository;
        _cache = cache;
    }

    /// <summary>
    /// Obtém pedido por ID (com cache inteligente)
    /// </summary>
    public async Task<Pedido?> ExecuteByIdAsync(long id)
    {
        var cacheKey = $"pedido:{id}";
        var cached = _cache.Get<Pedido>(cacheKey);
        if (cached != null)
            return cached;

        var pedido = await _pedidoRepository.ObterPorIdAsync(id);
        if (pedido == null)
            return null;

        _cache.SetWithTtl(cacheKey, pedido, TimeSpan.FromHours(6));
        return pedido;
    }

    /// <summary>
    /// Obtém pedido por número
    /// </summary>
    public async Task<Pedido?> ExecuteByNumeroAsync(int numero)
    {
        var pedidos = await _pedidoRepository.BuscarPorNumeroAsync(numero);
        if (pedidos.Count == 0)
            return null;

        return await ExecuteByIdAsync(pedidos[0].Id);
    }

    /// <summary>
    /// Invalida o cache de um pedido
    /// </summary>
    public void InvalidarCache(long? id = null)
    {
        if (id.HasValue)
            _cache.Remove($"pedido:{id}");
        else
            _cache.RemoveByPrefix("pedido:");
    }
}
