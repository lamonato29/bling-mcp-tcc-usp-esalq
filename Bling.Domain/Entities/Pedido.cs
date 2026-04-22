using Bling.Domain.Enums;

namespace Bling.Domain.Entities;

/// <summary>
/// Pedido de venda do Bling.
/// Contém os dados básicos retornados na listagem (GET /pedidos/vendas).
/// </summary>
public class Pedido : Entity
{
    public int Numero { get; set; }
    public string? NumeroLoja { get; set; }
    public DateTime? Data { get; set; }
    public DateTime? DataSaida { get; set; }
    public DateTime? DataPrevista { get; set; }
    public decimal TotalProdutos { get; set; }
    public decimal Total { get; set; }
    public string? NumeroPedidoCompra { get; set; }
    public decimal OutrasDespesas { get; set; }
    public string? Observacoes { get; set; }
    public string? ObservacoesInternas { get; set; }

    // Relacionamentos
    public PedidoContato? Contato { get; set; }
    public PedidoSituacao? Situacao { get; set; }
    public CanalVenda? Loja { get; set; }
    public PedidoDesconto? Desconto { get; set; }
    public PedidoCategoria? Categoria { get; set; }
    public PedidoNotaFiscal? NotaFiscal { get; set; }
    public PedidoTributacao? Tributacao { get; set; }
    public PedidoVendedor? Vendedor { get; set; }
    public PedidoIntermediador? Intermediador { get; set; }
    public PedidoTaxas? Taxas { get; set; }
    public PedidoTransporte? Transporte { get; set; }
    public List<PedidoItem>? Itens { get; set; }
    public List<PedidoParcela>? Parcelas { get; set; }

    /// <summary>
    /// Verifica se o pedido possui itens
    /// </summary>
    public bool PossuiItens => Itens is { Count: > 0 };

    /// <summary>
    /// Calcula o valor total dos itens
    /// </summary>
    public decimal CalcularTotalItens()
    {
        if (Itens == null) return 0;
        return Itens.Sum(i => i.Valor * i.Quantidade - i.Desconto);
    }
}

/// <summary>
/// Situação do pedido
/// </summary>
public class PedidoSituacao
{
    public long Id { get; set; }
    public int Valor { get; set; }
    public string? Nome { get; set; }
}

/// <summary>
/// Contato associado ao pedido
/// </summary>
public class PedidoContato
{
    public long Id { get; set; }
    public string? Nome { get; set; }
    public TipoPessoa? TipoPessoa { get; set; }
    public string? NumeroDocumento { get; set; }
}

/// <summary>
/// Categoria do pedido
/// </summary>
public class PedidoCategoria
{
    public long Id { get; set; }
}

/// <summary>
/// Nota fiscal vinculada ao pedido
/// </summary>
public class PedidoNotaFiscal
{
    public long Id { get; set; }
}

/// <summary>
/// Tributação do pedido
/// </summary>
public class PedidoTributacao
{
    public decimal TotalICMS { get; set; }
    public decimal TotalIPI { get; set; }
}

/// <summary>
/// Vendedor do pedido
/// </summary>
public class PedidoVendedor
{
    public long Id { get; set; }
}

/// <summary>
/// Intermediador do pedido (marketplace)
/// </summary>
public class PedidoIntermediador
{
    public string? Cnpj { get; set; }
    public string? NomeUsuario { get; set; }
}

/// <summary>
/// Taxas do pedido
/// </summary>
public class PedidoTaxas
{
    public decimal TaxaComissao { get; set; }
    public decimal CustoFrete { get; set; }
    public decimal ValorBase { get; set; }
}

/// <summary>
/// Desconto do pedido
/// </summary>
public class PedidoDesconto
{
    public decimal Valor { get; set; }
    public string? Unidade { get; set; }
}

/// <summary>
/// Item do pedido
/// </summary>
public class PedidoItem
{
    public long Id { get; set; }
    public string? Codigo { get; set; }
    public string? Unidade { get; set; }
    public decimal Quantidade { get; set; }
    public decimal Desconto { get; set; }
    public decimal Valor { get; set; }
    public decimal AliquotaIPI { get; set; }
    public string? Descricao { get; set; }
    public string? DescricaoDetalhada { get; set; }
    public PedidoItemProduto? Produto { get; set; }
    public PedidoItemComissao? Comissao { get; set; }
    public PedidoItemNaturezaOperacao? NaturezaOperacao { get; set; }
    
    /// <summary>
    /// Calcula o total do item (valor * quantidade - desconto)
    /// </summary>
    public decimal CalcularTotal() => Valor * Quantidade - Desconto;
}

public class PedidoItemProduto
{
    public long Id { get; set; }
    public ProdutoEstrutura? Estrutura { get; set; }
}

public class PedidoItemComissao
{
    public decimal Base { get; set; }
    public decimal Aliquota { get; set; }
    public decimal Valor { get; set; }
}

public class PedidoItemNaturezaOperacao
{
    public long Id { get; set; }
}

/// <summary>
/// Parcela de pagamento do pedido
/// </summary>
public class PedidoParcela
{
    public long Id { get; set; }
    public string? DataVencimento { get; set; }
    public decimal Valor { get; set; }
    public string? Observacoes { get; set; }
    public string? Caut { get; set; }
    public PedidoFormaPagamento? FormaPagamento { get; set; }
}

public class PedidoFormaPagamento
{
    public long Id { get; set; }
}

/// <summary>
/// Dados de transporte do pedido
/// </summary>
public class PedidoTransporte
{
    public TipoFrete FretePorConta { get; set; }
    public decimal Frete { get; set; }
    public int QuantidadeVolumes { get; set; }
    public decimal PesoBruto { get; set; }
    public int PrazoEntrega { get; set; }
    public TransporteContato? Contato { get; set; }
    public TransporteEtiqueta? Etiqueta { get; set; }
    public List<TransporteVolume>? Volumes { get; set; }
}

public class TransporteContato
{
    public long Id { get; set; }
    public string? Nome { get; set; }
}

public class TransporteEtiqueta
{
    public string? Nome { get; set; }
    public string? Endereco { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Municipio { get; set; }
    public string? Uf { get; set; }
    public string? Cep { get; set; }
    public string? Bairro { get; set; }
    public string? NomePais { get; set; }
}

public class TransporteVolume
{
    public long Id { get; set; }
    public string? Servico { get; set; }
    public string? CodigoRastreamento { get; set; }
}
