using Bling.Application.DTOs;
using Bling.Application.Interfaces;
using Bling.Application.UseCases.Pedidos;
using Bling.Domain.Entities;

namespace Bling.Application.UseCases.Analytics;

/// <summary>
/// Use Case: Análises agregadas de clientes (contatos)
/// </summary>
public class ContatoAnalyticsUseCase
{
    private readonly ListarPedidosUseCase _listarPedidosUseCase;
    private readonly IContatoRepository _contatoRepository;

    public ContatoAnalyticsUseCase(
        ListarPedidosUseCase listarPedidosUseCase,
        IContatoRepository contatoRepository)
    {
        _listarPedidosUseCase = listarPedidosUseCase;
        _contatoRepository = contatoRepository;
    }

    /// <summary>
    /// Resumo geral de clientes baseado nos pedidos
    /// </summary>
    public async Task<CustomerSummaryDto> GetSummaryAsync(PedidoFiltrosDto filtros)
    {
        var pedidos = await BuscarTodosPedidos(filtros);

        var clientesGroup = pedidos
            .Where(p => p.Contato != null && p.Contato.Id > 0)
            .GroupBy(p => p.Contato!.Id);

        var totalClientes = clientesGroup.Count();
        var totalPedidos = pedidos.Count;
        var valorTotal = pedidos.Sum(p => p.Total);

        return new CustomerSummaryDto
        {
            TotalClientesUnicos = totalClientes,
            TotalPedidos = totalPedidos,
            ValorTotal = valorTotal,
            TicketMedio = totalPedidos > 0 ? valorTotal / totalPedidos : 0,
            MediaComprasPorCliente = totalClientes > 0 ? (decimal)totalPedidos / totalClientes : 0
        };
    }

    /// <summary>
    /// Top N clientes que mais compraram
    /// </summary>
    public async Task<List<TopCustomerReportDto>> GetTopCustomersAsync(
        PedidoFiltrosDto filtros, int limite = 10, string ordenarPor = "valor")
    {
        var pedidos = await BuscarTodosPedidos(filtros);

        var clientesGroup = pedidos
            .Where(p => p.Contato != null && p.Contato.Id > 0)
            .GroupBy(p => p.Contato!.Id)
            .Select(g =>
            {
                var primeiro = g.First().Contato!;
                var valorTotal = g.Sum(p => p.Total);
                var qtd = g.Count();
                var datas = g.Where(p => p.Data.HasValue).Select(p => p.Data!.Value).ToList();
                return new TopCustomerReportDto
                {
                    IdContato = g.Key,
                    Nome = primeiro.Nome,
                    NumeroDocumento = primeiro.NumeroDocumento,
                    QuantidadePedidos = qtd,
                    ValorTotal = valorTotal,
                    TicketMedio = qtd > 0 ? valorTotal / qtd : 0,
                    PrimeiraCompra = datas.Count > 0 ? datas.Min().ToString("yyyy-MM-dd") : null,
                    UltimaCompra = datas.Count > 0 ? datas.Max().ToString("yyyy-MM-dd") : null
                };
            });

        return ordenarPor.ToLower() switch
        {
            "quantidade" => clientesGroup.OrderByDescending(c => c.QuantidadePedidos).Take(limite).ToList(),
            "ticket" => clientesGroup.OrderByDescending(c => c.TicketMedio).Take(limite).ToList(),
            _ => clientesGroup.OrderByDescending(c => c.ValorTotal).Take(limite).ToList()
        };
    }

    /// <summary>
    /// Histórico de compras de um cliente específico
    /// </summary>
    public async Task<CustomerHistoryDto?> GetCustomerHistoryAsync(
        PedidoFiltrosDto filtros, long? idContato = null, string? documento = null)
    {
        // Se temos documento mas não ID, buscar contato primeiro
        if (idContato == null && !string.IsNullOrEmpty(documento))
        {
            var contato = await _contatoRepository.BuscarPorDocumentoAsync(documento);
            idContato = contato?.Id;
        }

        if (idContato == null) return null;

        var filtrosCliente = new PedidoFiltrosDto
        {
            IdContato = idContato,
            DataInicial = filtros.DataInicial,
            DataFinal = filtros.DataFinal,
            Limite = filtros.Limite
        };

        var pedidos = await BuscarTodosPedidos(filtrosCliente);
        if (pedidos.Count == 0) return null;

        var contatoInfo = pedidos.First().Contato;
        var valorTotal = pedidos.Sum(p => p.Total);

        return new CustomerHistoryDto
        {
            IdContato = idContato.Value,
            Nome = contatoInfo?.Nome,
            NumeroDocumento = contatoInfo?.NumeroDocumento,
            TotalPedidos = pedidos.Count,
            ValorTotal = valorTotal,
            TicketMedio = pedidos.Count > 0 ? valorTotal / pedidos.Count : 0,
            Pedidos = pedidos
                .OrderByDescending(p => p.Data)
                .Take(50)
                .Select(p => new CustomerOrderDto
                {
                    IdPedido = p.Id,
                    NumeroPedido = p.Numero,
                    Data = p.Data?.ToString("yyyy-MM-dd"),
                    Total = p.Total,
                    Situacao = p.Situacao?.Valor.ToString()
                })
                .ToList()
        };
    }

    #region Helpers

    private async Task<List<Pedido>> BuscarTodosPedidos(PedidoFiltrosDto filtros)
    {
        return await _listarPedidosUseCase.ExecuteAsync(filtros, delayMs: 200, maxPaginas: int.MaxValue);
    }

    #endregion
}
