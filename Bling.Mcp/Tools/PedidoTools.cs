using System.ComponentModel;
using System.Text.Json;
using Bling.Application.DTOs;
using Bling.Application.Interfaces;
using Bling.Application.UseCases.Pedidos;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace Bling.Mcp.Tools;

/// <summary>
/// MCP Tools para o domínio de Pedidos de Venda.
/// Delega exclusivamente para os Use Cases da Application layer.
/// </summary>
[McpServerToolType]
public static class PedidoTools
{
    [McpServerTool(Name = "listar_pedidos"), Description(
        "Lista pedidos de venda do Bling com filtros opcionais. " +
        "Retorna uma lista de pedidos com id, número, total, data, contato e situação.")]
    public static async Task<string> ListarPedidos(
        ListarPedidosUseCase useCase,
        [Description("Data inicial (formato yyyy-MM-dd). Opcional.")] string? dataInicial = null,
        [Description("Data final (formato yyyy-MM-dd). Opcional.")] string? dataFinal = null,
        [Description("IDs das situações separados por vírgula. Opcional.")] string? idsSituacoes = null,
        [Description("Número específico do pedido. Opcional.")] int? numero = null,
        [Description("ID da loja/canal de venda. Opcional.")] long? idLoja = null,
        [Description("ID do contato/cliente. Opcional.")] long? idContato = null,
        [Description("Limite de resultados por página (máx 100). Padrão: 100.")] int limite = 100,
        [Description("Máximo de páginas a buscar. Padrão: 5.")] int maxPaginas = 5)
    {
        var filtros = new PedidoFiltrosDto
        {
            Limite = Math.Min(limite, 100),
            Numero = numero,
            IdLoja = idLoja,
            IdContato = idContato
        };

        if (DateTime.TryParse(dataInicial, out var di)) filtros.DataInicial = di;
        if (DateTime.TryParse(dataFinal, out var df)) filtros.DataFinal = df;
        if (!string.IsNullOrEmpty(idsSituacoes))
        {
            filtros.IdsSituacoes = idsSituacoes
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(long.Parse)
                .ToList();
        }

        var pedidos = await useCase.ExecuteAsync(filtros, maxPaginas: maxPaginas);

        return JsonSerializer.Serialize(new
        {
            total = pedidos.Count,
            pedidos = pedidos.Select(p => new
            {
                p.Id,
                p.Numero,
                p.NumeroLoja,
                p.Total,
                p.TotalProdutos,
                Data = p.Data?.ToString("yyyy-MM-dd"),
                Contato = p.Contato != null ? new { p.Contato.Id, p.Contato.Nome } : null,
                Situacao = p.Situacao != null ? new { p.Situacao.Id, p.Situacao.Valor } : null,
                Loja = p.Loja != null ? new { p.Loja.Id, p.Loja.Descricao } : null
            })
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool(Name = "buscar_pedido_por_numero_loja"), Description(
        "Busca pedidos pelo número do marketplace/loja (ex: número do pedido Shopee, Mercado Livre). " +
        "Retorna lista de pedidos que correspondem ao número informado.")]
    public static async Task<string> BuscarPorNumeroLoja(
        ListarPedidosUseCase useCase,
        [Description("Número(s) do pedido na loja/marketplace, separados por vírgula.")] string numerosLoja)
    {
        if (string.IsNullOrWhiteSpace(numerosLoja))
            return JsonSerializer.Serialize(new { erro = "Informe o(s) número(s) da loja." });

        var filtros = new PedidoFiltrosDto
        {
            Limite = 100,
            NumerosLojas = numerosLoja.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
        };

        var pedidos = await useCase.ExecuteAsync(filtros, maxPaginas: 3);

        return JsonSerializer.Serialize(new
        {
            total = pedidos.Count,
            pedidos = pedidos.Select(p => new
            {
                p.Id, p.Numero, p.NumeroLoja, p.Total,
                Data = p.Data?.ToString("yyyy-MM-dd"),
                Contato = p.Contato != null ? new { p.Contato.Id, p.Contato.Nome } : null,
                Situacao = p.Situacao != null ? new { p.Situacao.Id, p.Situacao.Valor } : null,
                Loja = p.Loja != null ? new { p.Loja.Id, p.Loja.Descricao } : null
            })
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool(Name = "listar_todos_pedidos"), Description(
        "Lista TODOS os pedidos de venda do período (paginação automática completa). " +
        "Operacao pesada executada em background. Use 'listar_pedidos' para consultas rapidas.")]
    public static async Task<string> ListarTodosPedidos(
        ListarPedidosUseCase useCase,
        IServiceProvider serviceProvider,
        [Description("Data inicial (formato yyyy-MM-dd). OBRIGATÓRIO.")] string dataInicial,
        [Description("Data final (formato yyyy-MM-dd). OBRIGATÓRIO.")] string dataFinal,
        [Description("IDs das situações separados por vírgula. Opcional.")] string? idsSituacoes = null,
        [Description("ID da loja/canal de venda. Opcional.")] long? idLoja = null)
    {
        var taskService = serviceProvider.GetRequiredService<IBackgroundTaskService>();
        if (!DateTime.TryParse(dataInicial, out var di) || !DateTime.TryParse(dataFinal, out var df))
            return JsonSerializer.Serialize(new { erro = "Informe dataInicial e dataFinal válidas." });

        var filtros = new PedidoFiltrosDto
        {
            Limite = 100,
            DataInicial = di,
            DataFinal = df,
            IdLoja = idLoja
        };

        if (!string.IsNullOrEmpty(idsSituacoes))
        {
            filtros.IdsSituacoes = idsSituacoes
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => long.TryParse(s, out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();
        }

        var result = await taskService.StartTaskAsync(
            "listar_todos_pedidos",
            new { dataInicial, dataFinal, idsSituacoes, idLoja },
            estimatedSeconds: 120,
            unitsDescription: "pedidos",
            unitsCount: 0,
            async (taskId, progress, ct) =>
            {
                progress(10, "Buscando todos os pedidos do período...", null);
                var pedidos = await useCase.ExecuteAsync(filtros, delayMs: 200, maxPaginas: int.MaxValue);
                progress(90, $"{pedidos.Count} pedidos encontrados. Serializando resultado...", null);
                return (object)new
                {
                    total = pedidos.Count,
                    pedidos = pedidos.Select(p => new
                    {
                        p.Id, p.Numero, p.NumeroLoja, p.Total,
                        Data = p.Data?.ToString("yyyy-MM-dd"),
                        Contato = p.Contato != null ? new { p.Contato.Id, p.Contato.Nome } : null,
                        Situacao = p.Situacao != null ? new { p.Situacao.Id, p.Situacao.Valor } : null,
                        Loja = p.Loja != null ? new { p.Loja.Id, p.Loja.Descricao } : null
                    })
                };
            });

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool(Name = "obter_pedido"), Description(
        "Obtém detalhes completos de um pedido de venda por ID ou número. " +
        "Retorna informações detalhadas incluindo itens, parcelas, transporte e mais.")]
    public static async Task<string> ObterPedido(
        ObterPedidoUseCase useCase,
        [Description("ID do pedido no Bling. Use este OU numero.")] long? id = null,
        [Description("Número do pedido. Use este OU id.")] int? numero = null)
    {
        if (id == null && numero == null)
            return JsonSerializer.Serialize(new { erro = "Informe 'id' ou 'numero' do pedido." });

        var pedido = id.HasValue
            ? await useCase.ExecuteByIdAsync(id.Value)
            : await useCase.ExecuteByNumeroAsync(numero!.Value);

        if (pedido == null)
            return JsonSerializer.Serialize(new { erro = "Pedido não encontrado." });

        return JsonSerializer.Serialize(new
        {
            pedido.Id,
            pedido.Numero,
            pedido.NumeroLoja,
            pedido.Total,
            pedido.TotalProdutos,
            pedido.OutrasDespesas,
            Data = pedido.Data?.ToString("yyyy-MM-dd"),
            DataSaida = pedido.DataSaida?.ToString("yyyy-MM-dd"),
            DataPrevista = pedido.DataPrevista?.ToString("yyyy-MM-dd"),
            pedido.Observacoes,
            pedido.ObservacoesInternas,
            Contato = pedido.Contato != null ? new { pedido.Contato.Id, pedido.Contato.Nome, pedido.Contato.NumeroDocumento } : null,
            Situacao = pedido.Situacao != null ? new { pedido.Situacao.Id, pedido.Situacao.Valor } : null,
            Loja = pedido.Loja != null ? new { pedido.Loja.Id, pedido.Loja.Descricao } : null,
            Desconto = pedido.Desconto != null ? new { pedido.Desconto.Valor, pedido.Desconto.Unidade } : null,
            Itens = pedido.Itens?.Select(i => new
            {
                i.Id, i.Codigo, i.Descricao, i.Quantidade, i.Valor, i.Desconto, i.Unidade
            }),
            Parcelas = pedido.Parcelas?.Select(p => new
            {
                p.Id, p.DataVencimento, p.Valor, p.Observacoes
            }),
            Transporte = pedido.Transporte != null ? new
            {
                FretePorConta = pedido.Transporte.FretePorConta.ToString(),
                pedido.Transporte.Frete,
                pedido.Transporte.QuantidadeVolumes,
                pedido.Transporte.PesoBruto
            } : null
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
