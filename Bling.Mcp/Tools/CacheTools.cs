using System.ComponentModel;
using System.Text.Json;
using Bling.Application.UseCases.Cache;
using ModelContextProtocol.Server;

namespace Bling.Mcp.Tools;

/// <summary>
/// MCP Tools para gerenciamento de cache.
/// </summary>
[McpServerToolType]
public static class CacheTools
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [McpServerTool(Name = "cache_stats"), Description(
        "Mostra estatísticas do cache: total de itens, quantidade por categoria (pedidos, produtos, canais, etc.).")]
    public static string GetCacheStats(CacheManagementUseCase useCase)
    {
        var stats = useCase.GetStats();
        return JsonSerializer.Serialize(stats, _jsonOptions);
    }

    [McpServerTool(Name = "cache_refresh_pedido"), Description(
        "Atualiza o cache de um pedido específico buscando novamente da API. " +
        "Útil quando um pedido foi alterado e os dados em cache estão desatualizados.")]
    public static async Task<string> RefreshPedido(
        CacheManagementUseCase useCase,
        [Description("ID do pedido no Bling.")] long id)
    {
        var ok = await useCase.RefreshPedidoAsync(id);
        return ok
            ? JsonSerializer.Serialize(new { sucesso = true, mensagem = $"Cache do pedido {id} atualizado." })
            : JsonSerializer.Serialize(new { sucesso = false, mensagem = $"Pedido {id} não encontrado na API." });
    }

    [McpServerTool(Name = "cache_refresh_pedidos_batch"), Description(
        "Atualiza o cache de múltiplos pedidos de uma vez. " +
        "Informe os IDs separados por vírgula.")]
    public static async Task<string> RefreshPedidosBatch(
        CacheManagementUseCase useCase,
        [Description("IDs dos pedidos separados por vírgula. Ex: 123,456,789")] string ids)
    {
        var idList = ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => long.TryParse(s, out var id) ? id : 0)
            .Where(id => id > 0)
            .ToList();

        if (idList.Count == 0)
            return JsonSerializer.Serialize(new { erro = "Nenhum ID válido informado." });

        var count = await useCase.RefreshPedidosBatchAsync(idList);
        return JsonSerializer.Serialize(new
        {
            sucesso = true,
            mensagem = $"{count} de {idList.Count} pedidos atualizados no cache."
        });
    }

    [McpServerTool(Name = "cache_invalidar"), Description(
        "Invalida (limpa) o cache de uma categoria específica ou todo o cache. " +
        "Categorias: pedidos, produtos, canais, situacoes, depositos, tudo.")]
    public static string InvalidarCache(
        CacheManagementUseCase useCase,
        [Description("Categoria a invalidar: pedidos, produtos, canais, situacoes, depositos, ou 'tudo'.")] string categoria)
    {
        var cat = categoria.ToLower().Trim();
        switch (cat)
        {
            case "pedidos":
                useCase.InvalidarPedidos();
                return JsonSerializer.Serialize(new { sucesso = true, mensagem = "Cache de pedidos invalidado." });
            case "produtos":
                useCase.InvalidarProdutos();
                return JsonSerializer.Serialize(new { sucesso = true, mensagem = "Cache de produtos invalidado." });
            case "canais":
                useCase.InvalidarCanais();
                return JsonSerializer.Serialize(new { sucesso = true, mensagem = "Cache de canais invalidado." });
            case "situacoes":
                useCase.InvalidarSituacoes();
                return JsonSerializer.Serialize(new { sucesso = true, mensagem = "Cache de situações invalidado." });
            case "depositos":
                useCase.InvalidarDepositos();
                return JsonSerializer.Serialize(new { sucesso = true, mensagem = "Cache de depósitos invalidado." });
            case "tudo":
                var count = useCase.InvalidarTudo();
                return JsonSerializer.Serialize(new { sucesso = true, mensagem = $"Cache completo invalidado ({count} itens removidos)." });
            default:
                return JsonSerializer.Serialize(new { erro = $"Categoria '{categoria}' desconhecida. Use: pedidos, produtos, canais, situacoes, depositos ou tudo." });
        }
    }
}
