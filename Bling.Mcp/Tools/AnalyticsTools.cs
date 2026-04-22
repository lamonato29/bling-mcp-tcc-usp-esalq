using System.ComponentModel;
using System.Text.Json;
using Bling.Application.DTOs;
using Bling.Application.Interfaces;
using Bling.Application.UseCases.Analytics;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace Bling.Mcp.Tools;

/// <summary>
/// MCP Tools para análises agregadas de vendas.
/// Operações pesadas são executadas em background tasks.
/// </summary>
[McpServerToolType]
public static class AnalyticsTools
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [McpServerTool(Name = "bling_analytics_summary"), Description(
        "Obtém RESUMO AGREGADO de vendas por período. " +
        "Retorna total de pedidos, valor total, agrupamento por situação, canal e mês. " +
        "ATENCAO: Busca todos os pedidos mas retorna apenas agregados.")]
    public static async Task<string> GetSummaryAsync(
        PedidoAnalyticsUseCase useCase,
        IServiceProvider serviceProvider,
        [Description("Data inicial do período (formato: yyyy-MM-dd).")] string? dataInicial = null,
        [Description("Data final do período (formato: yyyy-MM-dd).")] string? dataFinal = null,
        [Description("IDs das situações separados por vírgula (opcional).")] string? idsSituacoes = null,
        [Description("ID do canal de venda para filtrar (opcional).")] string? idLoja = null)
    {
        var taskService = serviceProvider.GetRequiredService<IBackgroundTaskService>();
        var filtros = BuildFiltros(dataInicial, dataFinal, idsSituacoes, idLoja);
        var result = await taskService.StartTaskAsync(
            "analytics_summary",
            new { dataInicial, dataFinal, idsSituacoes, idLoja },
            estimatedSeconds: 60,
            unitsDescription: "pedidos",
            unitsCount: 0,
            async (taskId, progress, ct) =>
            {
                progress(10, "Buscando pedidos do período...", null);
                var summary = await useCase.GetSummaryAsync(filtros);
                progress(80, $"Agrupando {summary.TotalPedidos} pedidos por situação, canal e mês...", null);
                return summary;
            });
        return JsonSerializer.Serialize(result, _jsonOptions);
    }

    [McpServerTool(Name = "bling_analytics_by_product"), Description(
        "Obtém vendas de um PRODUTO ESPECÍFICO por código ou descrição (busca parcial). " +
        "Retorna quantidade vendida, valor total, e lista dos últimos pedidos.")]
    public static async Task<string> GetSalesByProductAsync(
        PedidoAnalyticsUseCase useCase,
        IServiceProvider serviceProvider,
        [Description("Código ou descrição do produto para buscar.")] string codigoProduto,
        [Description("Data inicial do período (yyyy-MM-dd).")] string? dataInicial = null,
        [Description("Data final do período (yyyy-MM-dd).")] string? dataFinal = null,
        [Description("Máximo de pedidos a analisar. Padrão: Todos.")] int? maxPedidos = null)
    {
        var taskService = serviceProvider.GetRequiredService<IBackgroundTaskService>();
        if (string.IsNullOrWhiteSpace(codigoProduto))
            return JsonSerializer.Serialize(new { erro = "Informe o código ou descrição do produto." });

        var filtros = BuildFiltros(dataInicial, dataFinal, null, null);
        var max = maxPedidos ?? int.MaxValue;
        var result = await taskService.StartTaskAsync(
            "analytics_by_product",
            new { codigoProduto, dataInicial, dataFinal, maxPedidos },
            estimatedSeconds: 30,
            unitsDescription: "pedidos detalhados",
            unitsCount: 0,
            async (taskId, progress, ct) =>
            {
                var report = await useCase.GetSalesByProductAsync(filtros, codigoProduto, max, progress);
                progress(90, $"{report.QuantidadePedidos} pedidos com o produto encontrados. Finalizando...", null);
                return report;
            });
        return JsonSerializer.Serialize(result, _jsonOptions);
    }

    [McpServerTool(Name = "bling_analytics_top_products"), Description(
        "Obtém os N produtos mais VENDIDOS no período. " +
        "Retorna ranking ordenado por valor contendo código, descrição, quantidade e valor total.")]
    public static async Task<string> GetTopProductsAsync(
        PedidoAnalyticsUseCase useCase,
        IServiceProvider serviceProvider,
        [Description("Quantidade de produtos no ranking. Padrão: 10.")] int limite = 10,
        [Description("Data inicial (yyyy-MM-dd).")] string? dataInicial = null,
        [Description("Data final (yyyy-MM-dd).")] string? dataFinal = null,
        [Description("Máximo de pedidos a analisar. Padrão: Todos.")] int? maxPedidos = null)
    {
        var taskService = serviceProvider.GetRequiredService<IBackgroundTaskService>();
        var filtros = BuildFiltros(dataInicial, dataFinal, null, null);
        var max = maxPedidos ?? int.MaxValue;
        var result = await taskService.StartTaskAsync(
            "analytics_top_products",
            new { limite, dataInicial, dataFinal, maxPedidos },
            estimatedSeconds: 30,
            unitsDescription: "pedidos detalhados",
            unitsCount: 0,
            async (taskId, progress, ct) =>
            {
                var report = await useCase.GetTopProductsAsync(filtros, limite, max, progress);
                progress(90, $"Ranking calculado com {report.Count} SKUs únicos. Finalizando...", null);
                return report;
            });
        return JsonSerializer.Serialize(result, _jsonOptions);
    }

    [McpServerTool(Name = "bling_analytics_by_channel"), Description(
        "Obtém vendas AGRUPADAS por canal de venda/marketplace. " +
        "Retorna lista de canais ordenada por valor contendo nome, quantidade e valor total.")]
    public static async Task<string> GetSalesByChannelAsync(
        PedidoAnalyticsUseCase useCase,
        IServiceProvider serviceProvider,
        [Description("Data inicial (yyyy-MM-dd).")] string? dataInicial = null,
        [Description("Data final (yyyy-MM-dd).")] string? dataFinal = null,
        [Description("IDs das situações separados por vírgula (opcional).")] string? idsSituacoes = null)
    {
        var taskService = serviceProvider.GetRequiredService<IBackgroundTaskService>();
        var filtros = BuildFiltros(dataInicial, dataFinal, idsSituacoes, null);
        var result = await taskService.StartTaskAsync(
            "analytics_by_channel",
            new { dataInicial, dataFinal, idsSituacoes },
            estimatedSeconds: 60,
            unitsDescription: "pedidos",
            unitsCount: 0,
            async (taskId, progress, ct) =>
            {
                progress(10, "Buscando pedidos do período...", null);
                var report = await useCase.GetSalesByChannelAsync(filtros);
                progress(80, $"Agrupando {report.Sum(c => c.QuantidadePedidos)} pedidos em {report.Count} canais...", null);
                return report;
            });
        return JsonSerializer.Serialize(result, _jsonOptions);
    }

    [McpServerTool(Name = "bling_analytics_daily"), Description(
        "Obtém vendas DIÁRIAS no período. " +
        "Retorna lista de dias com data, quantidade de pedidos e valor total.")]
    public static async Task<string> GetDailySalesAsync(
        PedidoAnalyticsUseCase useCase,
        IServiceProvider serviceProvider,
        [Description("Data inicial (yyyy-MM-dd).")] string? dataInicial = null,
        [Description("Data final (yyyy-MM-dd).")] string? dataFinal = null,
        [Description("IDs das situações separados por vírgula (opcional).")] string? idsSituacoes = null,
        [Description("ID do canal de venda (opcional).")] string? idLoja = null)
    {
        var taskService = serviceProvider.GetRequiredService<IBackgroundTaskService>();
        var filtros = BuildFiltros(dataInicial, dataFinal, idsSituacoes, idLoja);
        var result = await taskService.StartTaskAsync(
            "analytics_daily",
            new { dataInicial, dataFinal, idsSituacoes, idLoja },
            estimatedSeconds: 60,
            unitsDescription: "pedidos",
            unitsCount: 0,
            async (taskId, progress, ct) =>
            {
                progress(10, "Buscando pedidos do período...", null);
                var report = await useCase.GetDailySalesAsync(filtros);
                progress(80, $"Agrupando vendas em {report.Count} dias. Finalizando...", null);
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
