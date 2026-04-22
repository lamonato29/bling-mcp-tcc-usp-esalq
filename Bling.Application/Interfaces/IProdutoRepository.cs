using Bling.Application.DTOs;
using Bling.Domain.Entities;

namespace Bling.Application.Interfaces;

/// <summary>
/// Port para acesso a dados de produtos.
/// </summary>
public interface IProdutoRepository
{
    /// <summary>
    /// Lista produtos com filtros e paginação automática
    /// </summary>
    Task<List<Produto>> ListarAsync(ProdutoFiltrosDto filtros, int delayMs = 400, int maxPaginas = int.MaxValue);

    /// <summary>
    /// Obtém detalhes completos de um produto por ID
    /// </summary>
    Task<Produto?> ObterPorIdAsync(long id);

    /// <summary>
    /// Busca produto por código
    /// </summary>
    Task<Produto?> BuscarPorCodigoAsync(string codigo);

    /// <summary>
    /// Obtém saldo de estoque de um produto por depósito
    /// </summary>
    Task<List<EstoqueItem>> ObterSaldoEstoqueAsync(long idProduto);
}
