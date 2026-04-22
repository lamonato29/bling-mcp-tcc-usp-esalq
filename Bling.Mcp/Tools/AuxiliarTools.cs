using System.ComponentModel;
using System.Text.Json;
using Bling.Application.UseCases.Auxiliares;
using ModelContextProtocol.Server;

namespace Bling.Mcp.Tools;

/// <summary>
/// MCP Tools para dados auxiliares (Canais de Venda, Situações, Depósitos).
/// Delega exclusivamente para os Use Cases da Application layer.
/// </summary>
[McpServerToolType]
public static class AuxiliarTools
{
    [McpServerTool(Name = "listar_canais_venda"), Description(
        "Lista os canais de venda (lojas) configurados no Bling. " +
        "Retorna id, descrição, tipo e situação de cada canal.")]
    public static async Task<string> ListarCanaisVenda(
        ListarCanaisVendaUseCase useCase)
    {
        var canais = await useCase.ExecuteAsync();

        return JsonSerializer.Serialize(new
        {
            total = canais.Count,
            canais = canais.Select(c => new
            {
                c.Id,
                c.Descricao,
                c.Tipo,
                c.Situacao
            })
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool(Name = "listar_situacoes"), Description(
        "Lista as situações customizadas de um módulo do Bling. " +
        "Retorna id, nome e cor de cada situação.")]
    public static async Task<string> ListarSituacoes(
        ListarSituacoesUseCase useCase,
        [Description("ID do módulo para filtrar situações. Opcional.")] long? idModulo = null)
    {
        var situacoes = await useCase.ExecuteAsync(idModulo);

        return JsonSerializer.Serialize(new
        {
            total = situacoes.Count,
            situacoes = situacoes.Select(s => new
            {
                s.Id,
                s.Nome,
                s.Cor
            })
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool(Name = "listar_depositos"), Description(
        "Lista os depósitos/estoques configurados no Bling. " +
        "Retorna id, descrição, situação, se é padrão e se desconta saldo.")]
    public static async Task<string> ListarDepositos(
        ListarDepositosUseCase useCase)
    {
        var depositos = await useCase.ExecuteAsync();

        return JsonSerializer.Serialize(new
        {
            total = depositos.Count,
            depositos = depositos.Select(d => new
            {
                d.Id,
                d.Descricao,
                d.Situacao,
                d.Padrao,
                d.DesconsiderarSaldo
            })
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
