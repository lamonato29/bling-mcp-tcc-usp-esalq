namespace Bling.Application.DTOs;

#region Pedido Analytics DTOs

public class PedidoSummaryDto
{
    public int TotalPedidos { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal ValorTotalProdutos { get; set; }
    public Dictionary<string, AggregateDto> PorSituacao { get; set; } = new();
    public Dictionary<string, AggregateDto> PorCanal { get; set; } = new();
    public Dictionary<string, AggregateDto> PorMes { get; set; } = new();
}

public class AggregateDto
{
    public int Quantidade { get; set; }
    public decimal ValorTotal { get; set; }
}

public class CanalVendaReportDto
{
    public long IdCanal { get; set; }
    public string NomeCanal { get; set; } = "";
    public int QuantidadePedidos { get; set; }
    public decimal ValorTotal { get; set; }
}

public class DailyReportDto
{
    public string Data { get; set; } = "";
    public int QuantidadePedidos { get; set; }
    public decimal ValorTotal { get; set; }
}

public class ProductSalesReportDto
{
    public string CodigoProduto { get; set; } = "";
    public int TotalPedidosAnalisados { get; set; }
    public int QuantidadePedidos { get; set; }
    public decimal QuantidadeVendida { get; set; }
    public decimal ValorTotal { get; set; }
    public string? Aviso { get; set; }
    public List<PedidoComProdutoDto> Pedidos { get; set; } = new();
}

public class PedidoComProdutoDto
{
    public long IdPedido { get; set; }
    public int NumeroPedido { get; set; }
    public string? Data { get; set; }
    public decimal Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }
}

public class TopProductReportDto
{
    public string? Codigo { get; set; }
    public string? Descricao { get; set; }
    public decimal QuantidadeVendida { get; set; }
    public decimal ValorTotal { get; set; }
    public int QuantidadePedidos { get; set; }
}

#endregion

#region Contato Analytics DTOs

public class CustomerSummaryDto
{
    public int TotalClientesUnicos { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal TicketMedio { get; set; }
    public decimal MediaComprasPorCliente { get; set; }
    public int TotalPedidos { get; set; }
}

public class TopCustomerReportDto
{
    public long IdContato { get; set; }
    public string? Nome { get; set; }
    public string? NumeroDocumento { get; set; }
    public int QuantidadePedidos { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal TicketMedio { get; set; }
    public string? PrimeiraCompra { get; set; }
    public string? UltimaCompra { get; set; }
}

public class CustomerHistoryDto
{
    public long IdContato { get; set; }
    public string? Nome { get; set; }
    public string? NumeroDocumento { get; set; }
    public int TotalPedidos { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal TicketMedio { get; set; }
    public List<CustomerOrderDto> Pedidos { get; set; } = new();
}

public class CustomerOrderDto
{
    public long IdPedido { get; set; }
    public int NumeroPedido { get; set; }
    public string? Data { get; set; }
    public decimal Total { get; set; }
    public string? Situacao { get; set; }
}

public class BalanceteDto
{
    public string Periodo { get; set; } = "";
    public BalanceteResumoDto Receitas { get; set; } = new();
    public BalanceteResumoDto Despesas { get; set; } = new();
    public BalanceteSaldoDto Saldo { get; set; } = new();
    public int QtdTitulosReceber { get; set; }
    public int QtdTitulosPagar { get; set; }
}

public class BalanceteResumoDto
{
    public decimal Total { get; set; }
    public decimal Realizado { get; set; }
    public decimal Previsto { get; set; }
}

public class BalanceteSaldoDto
{
    public decimal Geral { get; set; }
    public decimal Realizado { get; set; }
}

#endregion
