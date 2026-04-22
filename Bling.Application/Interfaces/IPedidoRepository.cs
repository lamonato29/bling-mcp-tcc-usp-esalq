using Bling.Application.DTOs;
using Bling.Domain.Entities;

namespace Bling.Application.Interfaces;

/// <summary>
/// Port para acesso a dados de pedidos de venda.
/// Abstrai a fonte de dados (API Bling, banco, etc.)
/// </summary>
public interface IPedidoRepository
{
    /// <summary>
    /// Lista pedidos com filtros e paginação automática
    /// </summary>
    Task<List<Pedido>> ListarAsync(PedidoFiltrosDto filtros, int delayMs = 400, int maxPaginas = int.MaxValue, Action<int>? onPage = null);

    /// <summary>
    /// Obtém detalhes completos de um pedido por ID
    /// </summary>
    Task<Pedido?> ObterPorIdAsync(long id);

    /// <summary>
    /// Busca pedidos por número
    /// </summary>
    Task<List<Pedido>> BuscarPorNumeroAsync(int numero);
}
