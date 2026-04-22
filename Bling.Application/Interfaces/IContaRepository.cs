using Bling.Application.DTOs;
using Bling.Domain.Entities;

namespace Bling.Application.Interfaces;

/// <summary>
/// Port para acesso a dados de contas a pagar e receber.
/// </summary>
public interface IContaRepository
{
    /// <summary>
    /// Lista contas a receber com filtros
    /// </summary>
    Task<List<ContaReceber>> ListarReceberAsync(ContaFiltrosDto filtros);

    /// <summary>
    /// Lista contas a pagar com filtros
    /// </summary>
    Task<List<ContaPagar>> ListarPagarAsync(ContaFiltrosDto filtros);
}
