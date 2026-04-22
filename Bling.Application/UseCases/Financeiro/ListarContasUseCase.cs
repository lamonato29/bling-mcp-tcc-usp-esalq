using Bling.Application.DTOs;
using Bling.Application.Interfaces;
using Bling.Domain.Entities;

namespace Bling.Application.UseCases.Financeiro;

/// <summary>
/// Use Case: Listar contas a receber com filtros
/// </summary>
public class ListarContasReceberUseCase
{
    private readonly IContaRepository _contaRepository;

    public ListarContasReceberUseCase(IContaRepository contaRepository)
    {
        _contaRepository = contaRepository;
    }

    public async Task<List<ContaReceber>> ExecuteAsync(ContaFiltrosDto filtros)
    {
        return await _contaRepository.ListarReceberAsync(filtros);
    }
}

/// <summary>
/// Use Case: Listar contas a pagar com filtros
/// </summary>
public class ListarContasPagarUseCase
{
    private readonly IContaRepository _contaRepository;

    public ListarContasPagarUseCase(IContaRepository contaRepository)
    {
        _contaRepository = contaRepository;
    }

    public async Task<List<ContaPagar>> ExecuteAsync(ContaFiltrosDto filtros)
    {
        return await _contaRepository.ListarPagarAsync(filtros);
    }
}
