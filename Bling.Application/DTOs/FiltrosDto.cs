namespace Bling.Application.DTOs;

/// <summary>
/// Filtros para listagem de pedidos de venda (Application DTO, sem lógica de API)
/// </summary>
public class PedidoFiltrosDto
{
    public DateTime? DataInicial { get; set; }
    public DateTime? DataFinal { get; set; }
    public DateTime? DataAlteracaoInicial { get; set; }
    public DateTime? DataAlteracaoFinal { get; set; }
    public DateTime? DataPrevistaInicial { get; set; }
    public DateTime? DataPrevistaFinal { get; set; }
    public List<long>? IdsSituacoes { get; set; }
    public int? Numero { get; set; }
    public long? IdLoja { get; set; }
    public long? IdVendedor { get; set; }
    public long? IdControleCaixa { get; set; }
    public List<string>? NumerosLojas { get; set; }
    public long? IdContato { get; set; }
    public int Pagina { get; set; } = 1;
    public int Limite { get; set; } = 100;
}

/// <summary>
/// Filtros para listagem de produtos (Application DTO)
/// </summary>
public class ProdutoFiltrosDto
{
    public int Pagina { get; set; } = 1;
    public int Limite { get; set; } = 100;
    public int? Criterio { get; set; }
    public string? Tipo { get; set; }
    public long? IdComponente { get; set; }
    public DateTime? DataInclusaoInicial { get; set; }
    public DateTime? DataInclusaoFinal { get; set; }
    public DateTime? DataAlteracaoInicial { get; set; }
    public DateTime? DataAlteracaoFinal { get; set; }
    public long? IdCategoria { get; set; }
    public long? IdLoja { get; set; }
    public string? Nome { get; set; }
    public List<long>? IdsProdutos { get; set; }
    public List<string>? Codigos { get; set; }
    public List<string>? Gtins { get; set; }
    public long? IdGrupoProduto { get; set; }
}

/// <summary>
/// Filtros para contas a pagar/receber
/// </summary>
public class ContaFiltrosDto
{
    public DateTime? DataEmissaoInicial { get; set; }
    public DateTime? DataEmissaoFinal { get; set; }
    public DateTime? DataVencimentoInicial { get; set; }
    public DateTime? DataVencimentoFinal { get; set; }
    public DateTime? DataPagamentoInicial { get; set; }
    public DateTime? DataPagamentoFinal { get; set; }
    public string? Situacao { get; set; }
    public long? IdContato { get; set; }
    public long? IdCategoria { get; set; }
    public int Pagina { get; set; } = 1;
    public int Limite { get; set; } = 100;
}

/// <summary>
/// Filtros para lotes
/// </summary>
public class LoteFiltrosDto
{
    public List<long>? IdsProdutos { get; set; }
    public List<long>? IdsLotes { get; set; }
    public List<long>? IdsDepositos { get; set; }
    public List<string>? CodigosLotes { get; set; }
    public int? Status { get; set; }
    public DateTime? DataValidadeInicial { get; set; }
    public DateTime? DataValidadeFinal { get; set; }
    public DateTime? DataFabricacaoInicial { get; set; }
    public DateTime? DataFabricacaoFinal { get; set; }
    public int Pagina { get; set; } = 1;
    public int Limite { get; set; } = 100;
}
