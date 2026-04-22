using Bling.Domain.Entities;

namespace Bling.Application.Interfaces;

/// <summary>
/// Port para acesso a dados auxiliares (canais, situações, depósitos).
/// </summary>
public interface IAuxiliarRepository
{
    /// <summary>
    /// Lista canais de venda
    /// </summary>
    Task<List<CanalVenda>> ListarCanaisVendaAsync();

    /// <summary>
    /// Lista situações customizadas de um módulo
    /// </summary>
    Task<List<SituacaoCustomizada>> ListarSituacoesAsync(long? idModulo = null);

    /// <summary>
    /// Lista depósitos de estoque
    /// </summary>
    Task<List<Deposito>> ListarDepositosAsync();

    /// <summary>
    /// Obtém dicionário de situações (ID → Nome) para resolução rápida
    /// </summary>
    Task<Dictionary<long, string>> ObterSituacoesDictAsync();
}
