using Bling.Application.DTOs;
using Bling.Application.Interfaces;
using Bling.Domain.Entities;

namespace Bling.Application.UseCases.Produtos;

/// <summary>
/// Use Case: Listar produtos com filtros
/// </summary>
public class ListarProdutosUseCase
{
    private readonly IProdutoRepository _produtoRepository;

    public ListarProdutosUseCase(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    public async Task<List<Produto>> ExecuteAsync(ProdutoFiltrosDto filtros, int delayMs = 400, int maxPaginas = int.MaxValue)
    {
        return await _produtoRepository.ListarAsync(filtros, delayMs, maxPaginas);
    }
}
