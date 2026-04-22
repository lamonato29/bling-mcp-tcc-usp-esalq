using System.ComponentModel;
using System.Text.Json;
using Bling.Application.DTOs;
using Bling.Application.Interfaces;
using Bling.Application.UseCases.Analytics;
using Bling.Application.UseCases.Financeiro;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace Bling.Mcp.Tools;

/// <summary>
/// MCP Tools para o domínio Financeiro (Contas a Receber e Contas a Pagar).
/// Delega exclusivamente para os Use Cases da Application layer.
/// </summary>
[McpServerToolType]
public static class FinanceiroTools
{
    [McpServerTool(Name = "listar_contas_receber"), Description(
        "Lista contas a receber do Bling com filtros opcionais. " +
        "Retorna uma lista com id, situação, valor, vencimento e contato.")]
    public static async Task<string> ListarContasReceber(
        ListarContasReceberUseCase useCase,
        [Description("Data de emissão inicial (formato yyyy-MM-dd). Opcional.")] string? dataEmissaoInicial = null,
        [Description("Data de emissão final (formato yyyy-MM-dd). Opcional.")] string? dataEmissaoFinal = null,
        [Description("Data de vencimento inicial (formato yyyy-MM-dd). Opcional.")] string? dataVencimentoInicial = null,
        [Description("Data de vencimento final (formato yyyy-MM-dd). Opcional.")] string? dataVencimentoFinal = null,
        [Description("Situação: 1=Em aberto, 2=Recebido, 3=Parcialmente recebido, 4=Devolvido, 5=Cancelado. Opcional.")] string? situacao = null,
        [Description("Número da página. Padrão: 1.")] int pagina = 1,
        [Description("Limite de resultados por página (máx 100). Padrão: 100.")] int limite = 100)
    {
        var filtros = new ContaFiltrosDto
        {
            Pagina = pagina,
            Limite = Math.Min(limite, 100),
            Situacao = situacao
        };

        if (DateTime.TryParse(dataEmissaoInicial, out var dei)) filtros.DataEmissaoInicial = dei;
        if (DateTime.TryParse(dataEmissaoFinal, out var def)) filtros.DataEmissaoFinal = def;
        if (DateTime.TryParse(dataVencimentoInicial, out var dvi)) filtros.DataVencimentoInicial = dvi;
        if (DateTime.TryParse(dataVencimentoFinal, out var dvf)) filtros.DataVencimentoFinal = dvf;

        var contas = await useCase.ExecuteAsync(filtros);

        return JsonSerializer.Serialize(new
        {
            total = contas.Count,
            contas = contas.Select(c => new
            {
                c.Id,
                c.Situacao,
                c.Valor,
                Vencimento = c.Vencimento?.ToString("yyyy-MM-dd"),
                c.NomeContato,
                c.IdContato
            })
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool(Name = "listar_contas_pagar"), Description(
        "Lista contas a pagar do Bling com filtros opcionais. " +
        "Retorna uma lista com id, situação, valor, vencimento e contato.")]
    public static async Task<string> ListarContasPagar(
        ListarContasPagarUseCase useCase,
        [Description("Data de emissão inicial (formato yyyy-MM-dd). Opcional.")] string? dataEmissaoInicial = null,
        [Description("Data de emissão final (formato yyyy-MM-dd). Opcional.")] string? dataEmissaoFinal = null,
        [Description("Data de vencimento inicial (formato yyyy-MM-dd). Opcional.")] string? dataVencimentoInicial = null,
        [Description("Data de vencimento final (formato yyyy-MM-dd). Opcional.")] string? dataVencimentoFinal = null,
        [Description("Situação: 1=Em aberto, 2=Pago, 3=Parcialmente pago, 4=Devolvido, 5=Cancelado. Opcional.")] string? situacao = null,
        [Description("Número da página. Padrão: 1.")] int pagina = 1,
        [Description("Limite de resultados por página (máx 100). Padrão: 100.")] int limite = 100)
    {
        var filtros = new ContaFiltrosDto
        {
            Pagina = pagina,
            Limite = Math.Min(limite, 100),
            Situacao = situacao
        };

        if (DateTime.TryParse(dataEmissaoInicial, out var dei)) filtros.DataEmissaoInicial = dei;
        if (DateTime.TryParse(dataEmissaoFinal, out var def)) filtros.DataEmissaoFinal = def;
        if (DateTime.TryParse(dataVencimentoInicial, out var dvi)) filtros.DataVencimentoInicial = dvi;
        if (DateTime.TryParse(dataVencimentoFinal, out var dvf)) filtros.DataVencimentoFinal = dvf;

        var contas = await useCase.ExecuteAsync(filtros);

        return JsonSerializer.Serialize(new
        {
            total = contas.Count,
            contas = contas.Select(c => new
            {
                c.Id,
                c.Situacao,
                c.Valor,
                Vencimento = c.Vencimento?.ToString("yyyy-MM-dd"),
                c.NomeContato,
                c.IdContato
            })
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool(Name = "obter_balancete"), Description(
        "Obtém balancete simplificado do período, comparando contas a receber vs contas a pagar. " +
        "Retorna receitas (total/realizado/previsto), despesas, saldo geral e saldo realizado. " +
        "Operacao pesada executada em background (busca todas as contas do periodo).")]
    public static async Task<string> GetBalancete(
        BalanceteUseCase useCase,
        IServiceProvider serviceProvider,
        [Description("Data inicial do período (formato yyyy-MM-dd).")] string dataInicial,
        [Description("Data final do período (formato yyyy-MM-dd).")] string dataFinal)
    {
        var taskService = serviceProvider.GetRequiredService<IBackgroundTaskService>();
        if (!DateTime.TryParse(dataInicial, out var di) || !DateTime.TryParse(dataFinal, out var df))
            return JsonSerializer.Serialize(new { erro = "Informe dataInicial e dataFinal válidas (yyyy-MM-dd)." });

        var result = await taskService.StartTaskAsync(
            "obter_balancete",
            new { dataInicial, dataFinal },
            estimatedSeconds: 60,
            unitsDescription: "títulos",
            unitsCount: 0,
            async (taskId, progress, ct) =>
            {
                progress(10, "Buscando contas a receber e a pagar em paralelo...", null);
                var balancete = await useCase.ExecuteAsync(di, df);
                progress(90, $"Consolidando balancete ({balancete.QtdTitulosReceber} receber / {balancete.QtdTitulosPagar} pagar)...", null);
                return (object)balancete;
            });

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }
}
