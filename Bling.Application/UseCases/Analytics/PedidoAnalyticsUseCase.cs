using Bling.Application.DTOs;
using Bling.Application.Interfaces;
using Bling.Application.UseCases.Pedidos;
using Bling.Domain.Entities;

namespace Bling.Application.UseCases.Analytics;

/// <summary>
/// Use Case: Resumo agregado de vendas (Summary, ByChannel, Daily, TopProducts)
/// </summary>
public class PedidoAnalyticsUseCase
{
    private readonly ListarPedidosUseCase _listarPedidosUseCase;
    private readonly ObterPedidoUseCase _obterPedidoUseCase;
    private readonly IAuxiliarRepository _auxiliarRepository;
    private readonly ICacheService _cache;

    public PedidoAnalyticsUseCase(
        ListarPedidosUseCase listarPedidosUseCase,
        ObterPedidoUseCase obterPedidoUseCase,
        IAuxiliarRepository auxiliarRepository,
        ICacheService cache)
    {
        _listarPedidosUseCase = listarPedidosUseCase;
        _obterPedidoUseCase = obterPedidoUseCase;
        _auxiliarRepository = auxiliarRepository;
        _cache = cache;
    }

    /// <summary>
    /// Resumo geral de vendas (por situação, canal, mês)
    /// </summary>
    public async Task<PedidoSummaryDto> GetSummaryAsync(PedidoFiltrosDto filtros)
    {
        var pedidos = await BuscarTodosPedidos(filtros);

        var situacoesDict = await BuscarSituacoesDict();

        return new PedidoSummaryDto
        {
            TotalPedidos = pedidos.Count,
            ValorTotal = pedidos.Sum(p => p.Total),
            ValorTotalProdutos = pedidos.Sum(p => p.TotalProdutos),
            PorSituacao = pedidos
                .GroupBy(p => p.Situacao?.Id ?? 0)
                .ToDictionary(
                    g => situacoesDict.GetValueOrDefault(g.Key, $"ID {g.Key}"),
                    g => new AggregateDto { Quantidade = g.Count(), ValorTotal = g.Sum(p => p.Total) }),
            PorCanal = pedidos
                .Where(p => p.Loja != null)
                .GroupBy(p => p.Loja!.Descricao ?? p.Loja.Tipo ?? $"ID {p.Loja.Id}")
                .ToDictionary(
                    g => g.Key,
                    g => new AggregateDto { Quantidade = g.Count(), ValorTotal = g.Sum(p => p.Total) }),
            PorMes = pedidos
                .Where(p => p.Data.HasValue)
                .GroupBy(p => p.Data!.Value.ToString("yyyy-MM"))
                .OrderBy(g => g.Key)
                .ToDictionary(
                    g => g.Key,
                    g => new AggregateDto { Quantidade = g.Count(), ValorTotal = g.Sum(p => p.Total) })
        };
    }

    /// <summary>
    /// Vendas agrupadas por canal de venda
    /// </summary>
    public async Task<List<CanalVendaReportDto>> GetSalesByChannelAsync(PedidoFiltrosDto filtros)
    {
        var pedidos = await BuscarTodosPedidos(filtros);

        return pedidos
            .Where(p => p.Loja != null)
            .GroupBy(p => new { p.Loja!.Id, Nome = p.Loja.Descricao ?? p.Loja.Tipo ?? $"ID {p.Loja.Id}" })
            .Select(g => new CanalVendaReportDto
            {
                IdCanal = g.Key.Id,
                NomeCanal = g.Key.Nome,
                QuantidadePedidos = g.Count(),
                ValorTotal = g.Sum(p => p.Total)
            })
            .OrderByDescending(c => c.ValorTotal)
            .ToList();
    }

    /// <summary>
    /// Vendas diárias no período
    /// </summary>
    public async Task<List<DailyReportDto>> GetDailySalesAsync(PedidoFiltrosDto filtros)
    {
        var pedidos = await BuscarTodosPedidos(filtros);

        return pedidos
            .Where(p => p.Data.HasValue)
            .GroupBy(p => p.Data!.Value.Date)
            .Select(g => new DailyReportDto
            {
                Data = g.Key.ToString("yyyy-MM-dd"),
                QuantidadePedidos = g.Count(),
                ValorTotal = g.Sum(p => p.Total)
            })
            .OrderBy(d => d.Data)
            .ToList();
    }

    /// <summary>
    /// Vendas de um produto específico (busca detalhes dos pedidos via repository)
    /// </summary>
    public async Task<ProductSalesReportDto> GetSalesByProductAsync(
        PedidoFiltrosDto filtros, string codigoProduto, int maxPedidos = int.MaxValue,
        Action<int, string, int?>? progress = null)
    {
        progress?.Invoke(5, "Buscando lista de pedidos do período...", null);
        var pedidosBase = await BuscarTodosPedidos(filtros, onPage: count =>
        {
            progress?.Invoke(5, $"Buscando pedidos... {count} encontrados", null);
        });

        var report = new ProductSalesReportDto
        {
            CodigoProduto = codigoProduto,
            TotalPedidosAnalisados = pedidosBase.Count
        };

        var pedidosParaDetalhar = maxPedidos == int.MaxValue
            ? pedidosBase
            : pedidosBase.Take(maxPedidos).ToList();

        var pedidosComProduto = new List<PedidoComProdutoDto>();
        var total = pedidosParaDetalhar.Count;
        var last = -1;

        // Atualiza estimativa dinâmica (2 pedidos por segundo + 30s buffer)
        progress?.Invoke(12, $"{total} pedidos encontrados. Iniciando análise detalhada...", (total / 2) + 30);

        for (var i = 0; i < total; i++)
        {
            var pedidoBase = pedidosParaDetalhar[i];
            var pedidoDetalhe = await _obterPedidoUseCase.ExecuteByIdAsync(pedidoBase.Id);
            if (pedidoDetalhe?.Itens == null) continue;

            foreach (var item in pedidoDetalhe.Itens)
            {
                if ((item.Codigo?.Contains(codigoProduto, StringComparison.OrdinalIgnoreCase) == true) ||
                    (item.Descricao?.Contains(codigoProduto, StringComparison.OrdinalIgnoreCase) == true))
                {
                    var totalItem = item.Valor * item.Quantidade - item.Desconto;

                    report.QuantidadeVendida += item.Quantidade;
                    report.ValorTotal += totalItem;
                    report.QuantidadePedidos++;

                    pedidosComProduto.Add(new PedidoComProdutoDto
                    {
                        IdPedido = pedidoBase.Id,
                        NumeroPedido = pedidoBase.Numero,
                        Data = pedidoBase.Data?.ToString("yyyy-MM-dd"),
                        Quantidade = item.Quantidade,
                        ValorUnitario = item.Valor,
                        ValorTotal = totalItem
                    });
                    break;
                }
            }

            // Reporta a cada 5% de avanço (faixa 15–85%)
            var pct = total > 0 ? 15 + (int)((i + 1) * 70.0 / total) : 15;
            if (pct != last)
            {
                progress?.Invoke(pct, $"Analisando pedido {i + 1}/{total} — {report.QuantidadePedidos} encontrados até agora...", null);
                last = pct;
            }

            await Task.Delay(50);
        }

        report.Pedidos = pedidosComProduto.OrderByDescending(p => p.Data).Take(50).ToList();

        if (pedidosBase.Count > maxPedidos && maxPedidos != int.MaxValue)
            report.Aviso = $"Analisados {maxPedidos} de {pedidosBase.Count} pedidos. Valores podem estar incompletos.";

        return report;
    }

    /// <summary>
    /// Top N produtos mais vendidos
    /// </summary>
    public async Task<List<TopProductReportDto>> GetTopProductsAsync(
        PedidoFiltrosDto filtros, int limite = 10, int maxPedidos = int.MaxValue,
        Action<int, string, int?>? progress = null)
    {
        progress?.Invoke(5, "Buscando lista de pedidos do período...", null);
        var pedidosBase = await BuscarTodosPedidos(filtros, onPage: count =>
        {
            progress?.Invoke(5, $"Buscando pedidos... {count} encontrados", null);
        });
        var produtosDict = new Dictionary<string, TopProductReportDto>();

        var pedidosParaDetalhar = maxPedidos == int.MaxValue
            ? pedidosBase
            : pedidosBase.Take(maxPedidos).ToList();

        var total = pedidosParaDetalhar.Count;
        var last = -1;

        // Atualiza estimativa dinâmica (2 pedidos por segundo + 30s buffer)
        progress?.Invoke(12, $"{total} pedidos encontrados. Analisando itens...", (total / 2) + 30);

        for (var i = 0; i < total; i++)
        {
            var pedidoBase = pedidosParaDetalhar[i];
            var pedidoDetalhe = await _obterPedidoUseCase.ExecuteByIdAsync(pedidoBase.Id);
            if (pedidoDetalhe?.Itens == null) continue;

            foreach (var item in pedidoDetalhe.Itens)
            {
                var key = item.Codigo ?? item.Descricao ?? $"Item {item.Id}";

                if (!produtosDict.TryGetValue(key, out var produto))
                {
                    produto = new TopProductReportDto { Codigo = item.Codigo, Descricao = item.Descricao };
                    produtosDict[key] = produto;
                }

                produto.QuantidadeVendida += item.Quantidade;
                produto.ValorTotal += item.Valor * item.Quantidade - item.Desconto;
                produto.QuantidadePedidos++;
            }

            // Reporta a cada 5% de avanço (faixa 15–85%)
            var pct = total > 0 ? 15 + (int)((i + 1) * 70.0 / total) : 15;
            if (pct != last)
            {
                progress?.Invoke(pct, $"Analisando pedido {i + 1}/{total} — {produtosDict.Count} SKUs mapeados...", null);
                last = pct;
            }

            await Task.Delay(50);
        }

        return produtosDict.Values
            .OrderByDescending(p => p.ValorTotal)
            .Take(limite)
            .ToList();
    }

    #region Helpers

    private async Task<List<Pedido>> BuscarTodosPedidos(PedidoFiltrosDto filtros, Action<int>? onPage = null)
    {
        return await _listarPedidosUseCase.ExecuteAsync(filtros, delayMs: 200, maxPaginas: int.MaxValue, onPage);
    }

    private async Task<Dictionary<long, string>> BuscarSituacoesDict()
    {
        var cached = _cache.Get<Dictionary<long, string>>("situacoes_dict");
        if (cached != null) return cached;

        var dict = await _auxiliarRepository.ObterSituacoesDictAsync();
        _cache.SetWithTtl("situacoes_dict", dict, TimeSpan.FromHours(12));
        return dict;
    }

    #endregion
}
