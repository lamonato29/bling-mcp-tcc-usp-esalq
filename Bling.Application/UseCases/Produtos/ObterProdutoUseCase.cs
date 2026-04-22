using Bling.Application.Interfaces;
using Bling.Domain.Entities;

namespace Bling.Application.UseCases.Produtos;

/// <summary>
/// Use Case: Obter produto por ID ou código com cache
/// </summary>
public class ObterProdutoUseCase
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly ICacheService _cache;

    public ObterProdutoUseCase(IProdutoRepository produtoRepository, ICacheService cache)
    {
        _produtoRepository = produtoRepository;
        _cache = cache;
    }

    public async Task<Produto?> ExecuteByIdAsync(long id)
    {
        var cacheKey = $"produto:{id}";
        var cached = _cache.Get<Produto>(cacheKey);
        if (cached != null)
            return cached;

        var produto = await _produtoRepository.ObterPorIdAsync(id);
        if (produto == null)
            return null;

        _cache.Set(cacheKey, produto);
        return produto;
    }

    public async Task<Produto?> ExecuteByCodigoAsync(string codigo)
    {
        var produto = await _produtoRepository.BuscarPorCodigoAsync(codigo);
        return produto;
    }
}
