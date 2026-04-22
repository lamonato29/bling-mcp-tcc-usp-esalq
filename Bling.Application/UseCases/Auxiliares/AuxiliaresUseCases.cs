using Bling.Application.Interfaces;
using Bling.Domain.Entities;

namespace Bling.Application.UseCases.Auxiliares;

/// <summary>
/// Use Case: Listar canais de venda
/// </summary>
public class ListarCanaisVendaUseCase
{
    private readonly IAuxiliarRepository _auxiliarRepository;
    private readonly ICacheService _cache;

    public ListarCanaisVendaUseCase(IAuxiliarRepository auxiliarRepository, ICacheService cache)
    {
        _auxiliarRepository = auxiliarRepository;
        _cache = cache;
    }

    public async Task<List<CanalVenda>> ExecuteAsync()
    {
        var cacheKey = "canais_venda";
        var cached = _cache.Get<List<CanalVenda>>(cacheKey);
        if (cached != null)
            return cached;

        var canais = await _auxiliarRepository.ListarCanaisVendaAsync();
        _cache.Set(cacheKey, canais);
        return canais;
    }
}

/// <summary>
/// Use Case: Listar situações de um módulo
/// </summary>
public class ListarSituacoesUseCase
{
    private readonly IAuxiliarRepository _auxiliarRepository;
    private readonly ICacheService _cache;

    public ListarSituacoesUseCase(IAuxiliarRepository auxiliarRepository, ICacheService cache)
    {
        _auxiliarRepository = auxiliarRepository;
        _cache = cache;
    }

    public async Task<List<SituacaoCustomizada>> ExecuteAsync(long? idModulo = null)
    {
        var cacheKey = idModulo.HasValue ? $"situacoes:modulo:{idModulo}" : "situacoes:all";
        var cached = _cache.Get<List<SituacaoCustomizada>>(cacheKey);
        if (cached != null)
            return cached;

        var situacoes = await _auxiliarRepository.ListarSituacoesAsync(idModulo);
        _cache.Set(cacheKey, situacoes);
        return situacoes;
    }
}

/// <summary>
/// Use Case: Listar depósitos
/// </summary>
public class ListarDepositosUseCase
{
    private readonly IAuxiliarRepository _auxiliarRepository;
    private readonly ICacheService _cache;

    public ListarDepositosUseCase(IAuxiliarRepository auxiliarRepository, ICacheService cache)
    {
        _auxiliarRepository = auxiliarRepository;
        _cache = cache;
    }

    public async Task<List<Deposito>> ExecuteAsync()
    {
        var cacheKey = "depositos";
        var cached = _cache.Get<List<Deposito>>(cacheKey);
        if (cached != null) return cached;

        var depositos = await _auxiliarRepository.ListarDepositosAsync();
        _cache.Set(cacheKey, depositos);
        return depositos;
    }
}
