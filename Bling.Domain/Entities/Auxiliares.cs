namespace Bling.Domain.Entities;

/// <summary>
/// Canal de venda (loja/marketplace)
/// </summary>
public class CanalVenda : Entity
{
    public string? Descricao { get; set; }
    public string? Tipo { get; set; }
    /// <summary>
    /// Situação do canal: 1 = Habilitado, 2 = Desabilitado
    /// </summary>
    public int Situacao { get; set; }

    public bool EstaHabilitado => Situacao == 1;
    public string SituacaoDescricao => EstaHabilitado ? "Habilitado" : "Desabilitado";
}

/// <summary>
/// Canal de venda com detalhes (filiais)
/// </summary>
public class CanalVendaDetalhado : CanalVenda
{
    public List<CanalVendaFilial>? Filiais { get; set; }
}

public class CanalVendaFilial
{
    public string? Cnpj { get; set; }
    public string? UnidadeNegocio { get; set; }
    public long? DepositoId { get; set; }
    public bool Padrao { get; set; }
}

/// <summary>
/// Vendedor
/// </summary>
public class Vendedor : Entity
{
    public decimal? DescontoLimite { get; set; }
    public bool? Lojista { get; set; }
    public decimal? Comissao { get; set; }
    public VendedorContato? Contato { get; set; }
    public VendedorLoja? Loja { get; set; }
}

public class VendedorContato
{
    public long Id { get; set; }
    public string? Nome { get; set; }
    public string? Situacao { get; set; }
}

public class VendedorLoja
{
    public long Id { get; set; }
    public string? Descricao { get; set; }
}

/// <summary>
/// Situação customizada de um módulo
/// </summary>
public class SituacaoCustomizada : Entity
{
    public long? IdModuloSistema { get; set; }
    public string? Nome { get; set; }
    public long? IdHerdado { get; set; }
    public string? Cor { get; set; }
}

/// <summary>
/// Módulo do sistema
/// </summary>
public class Modulo : Entity
{
    public string? Nome { get; set; }
    public string? Descricao { get; set; }
}

/// <summary>
/// Categoria de produto
/// </summary>
public class Categoria : Entity
{
    public long? IdCategoriaPai { get; set; }
    public string? Descricao { get; set; }
}

/// <summary>
/// Grupo de produtos (tags)
/// </summary>
public class GrupoProduto : Entity
{
    public string? Nome { get; set; }
    public GrupoProdutoPai? GrupoProdutoPai { get; set; }
}

public class GrupoProdutoPai
{
    public long Id { get; set; }
    public string? Nome { get; set; }
}

/// <summary>
/// Forma de pagamento
/// </summary>
public class FormaPagamento : Entity
{
    public string? Descricao { get; set; }
    public int? TipoPagamento { get; set; }
    public string? Situacao { get; set; }
    public bool? Padrao { get; set; }
    public string? CondicaoPagamento { get; set; }
}

/// <summary>
/// Lote de produto
/// </summary>
public class Lote : Entity
{
    public string? Codigo { get; set; }
    public long? IdProduto { get; set; }
    public LoteProduto? Produto { get; set; }
    public Deposito? Deposito { get; set; }
    public DateTime? DataValidade { get; set; }
    public DateTime? DataFabricacao { get; set; }
    public decimal Quantidade { get; set; }
    public decimal? Saldo { get; set; }
    public string? Observacoes { get; set; }
    /// <summary>
    /// Status: 1=Ativo, 2=Inativo
    /// </summary>
    public int Status { get; set; } = 1;

    public bool EstaAtivo => Status == 1;
}

public class LoteProduto
{
    public long Id { get; set; }
    public string? Codigo { get; set; }
    public string? Nome { get; set; }
}
