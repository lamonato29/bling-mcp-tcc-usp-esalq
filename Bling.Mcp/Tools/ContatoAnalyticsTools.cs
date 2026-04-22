using System.ComponentModel;
using System.Text.Json;
using Bling.Application.DTOs;
using Bling.Application.Interfaces;
using Bling.Application.UseCases.Analytics;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace Bling.Mcp.Tools;

/// <summary>
/// MCP Tools para análises agregadas de clientes (contatos).
/// Operações pesadas são executadas em background tasks.
/// </summary>
[McpServerToolType]
public static class ContatoAnalyticsTools
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [McpServerTool(Name = "bling_contato_analytics_summary"), Description(
        "Obtém RESUMO AGREGADO de clientes baseado nos pedidos de venda. " +
        "Retorna total de clientes únicos, valor total, ticket médio e média de compras por cliente.")]
    public static async Task<string> GetSummaryAsync(
        ContatoAnalyticsUseCase useCase,
        IServiceProvider serviceProvider,
        [Description("Data inicial do período (yyyy-MM-dd).")] string? dataInicial = null,
        [Description("Data final do período (yyyy-MM-dd).")] string? dataFinal = null,
        [Description("IDs das situações separados por vírgula (opcional).")] string? idsSituacoes = null,
        [Description("ID do canal de venda (opcional).")] string? idLoja = null)
    {
        var taskService = serviceProvider.GetRequiredService<IBackgroundTaskService>();
        var filtros = BuildFiltros(dataInicial, dataFinal, idsSituacoes, idLoja);
        var result = await taskService.StartTaskAsync(
            "contato_analytics_summary",
            new { dataInicial, dataFinal, idsSituacoes, idLoja },
            estimatedSeconds: 60,
            unitsDescription: "pedidos",
            unitsCount: 0,
            async (taskId, progress, ct) =>
            {
                progress(10, "Buscando pedidos do período...", null);
                var summary = await useCase.GetSummaryAsync(filtros);
                progress(90, $"Analisados {summary.TotalClientesUnicos} clientes únicos. Finalizando...", null);
                return summary;
            });
        return JsonSerializer.Serialize(result, _jsonOptions);
    }

    [McpServerTool(Name = "bling_contato_analytics_top_customers"), Description(
        "Obtém os N clientes que MAIS COMPRARAM no período. " +
        "Retorna ranking com nome, documento, quantidade de pedidos, valor total, ticket médio e datas.")]
    public static async Task<string> GetTopCustomersAsync(
        ContatoAnalyticsUseCase useCase,
        IServiceProvider serviceProvider,
        [Description("Quantidade de clientes no ranking. Padrão: 10.")] int limite = 10,
        [Description("Ordenar por: 'valor' (padrão), 'quantidade' ou 'ticket'.")] string? ordenarPor = null,
        [Description("Data inicial (yyyy-MM-dd).")] string? dataInicial = null,
        [Description("Data final (yyyy-MM-dd).")] string? dataFinal = null,
        [Description("IDs das situações separados por vírgula. Padrão: 6,9,15,24.")] string? idsSituacoes = null,
        [Description("ID do canal de venda (opcional).")] string? idLoja = null)
    {
        var taskService = serviceProvider.GetRequiredService<IBackgroundTaskService>();
        if (string.IsNullOrWhiteSpace(idsSituacoes))
            idsSituacoes = "6,9,15,24";

        var filtros = BuildFiltros(dataInicial, dataFinal, idsSituacoes, idLoja);
        var result = await taskService.StartTaskAsync(
            "contato_analytics_top_customers",
            new { limite, ordenarPor, dataInicial, dataFinal, idsSituacoes, idLoja },
            estimatedSeconds: 90,
            unitsDescription: "pedidos",
            unitsCount: 0,
            async (taskId, progress, ct) =>
            {
                progress(10, "Buscando pedidos do período...", null);
                var report = await useCase.GetTopCustomersAsync(filtros, limite, ordenarPor ?? "valor");
                progress(90, $"Ranking calculado com {report.Count} clientes. Finalizando...", null);
                return report;
            });
        return JsonSerializer.Serialize(result, _jsonOptions);
    }

    [McpServerTool(Name = "bling_contato_analytics_by_customer"), Description(
        "Obtém HISTÓRICO DE COMPRAS de um cliente específico por ID ou documento (CPF/CNPJ). " +
        "Retorna dados do cliente, total de pedidos, valor total e lista dos últimos 50 pedidos.")]
    public static async Task<string> GetCustomerHistoryAsync(
        ContatoAnalyticsUseCase useCase,
        IServiceProvider serviceProvider,
        [Description("ID do contato no Bling (opcional se informar documento).")] long? idContato = null,
        [Description("CPF ou CNPJ do cliente (opcional se informar ID).")] string? documento = null,
        [Description("Data inicial (yyyy-MM-dd).")] string? dataInicial = null,
        [Description("Data final (yyyy-MM-dd).")] string? dataFinal = null)
    {
        var taskService = serviceProvider.GetRequiredService<IBackgroundTaskService>();
        if (idContato == null && string.IsNullOrWhiteSpace(documento))
            return JsonSerializer.Serialize(new { erro = "Informe o ID do contato OU o documento (CPF/CNPJ)." });

        var filtros = BuildFiltros(dataInicial, dataFinal, null, null);
        var result = await taskService.StartTaskAsync(
            "contato_analytics_by_customer",
            new { idContato, documento, dataInicial, dataFinal },
            estimatedSeconds: 60,
            unitsDescription: "pedidos",
            unitsCount: 0,
            async (taskId, progress, ct) =>
            {
                progress(10, "Buscando histórico de pedidos do cliente...", null);
                var report = await useCase.GetCustomerHistoryAsync(filtros, idContato, documento);
                if (report == null)
                    return new { erro = "Cliente não encontrado." } as object;
                progress(90, $"Encontrados {report.TotalPedidos} pedidos. Finalizando...", null);
                return report;
            });
        return JsonSerializer.Serialize(result, _jsonOptions);
    }

    #region Helpers

    private static PedidoFiltrosDto BuildFiltros(
        string? dataInicial, string? dataFinal, string? idsSituacoes, string? idLoja)
    {
        var filtros = new PedidoFiltrosDto();

        if (DateTime.TryParse(dataInicial, out var di)) filtros.DataInicial = di;
        if (DateTime.TryParse(dataFinal, out var df)) filtros.DataFinal = df;
        if (long.TryParse(idLoja, out var loja)) filtros.IdLoja = loja;

        if (!string.IsNullOrWhiteSpace(idsSituacoes))
        {
            filtros.IdsSituacoes = idsSituacoes.Split(',')
                .Select(s => long.TryParse(s.Trim(), out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();
        }

        return filtros;
    }

    #endregion
}
