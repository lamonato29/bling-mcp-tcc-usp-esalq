using Bling.Domain.Enums;

namespace Bling.Domain.Entities;

/// <summary>
/// Produto do Bling
/// </summary>
public class Produto : Entity
{
    public string? Codigo { get; set; }
    public string? Nome { get; set; }
    public decimal Preco { get; set; }
    public decimal PrecoCusto { get; set; }
    public string? Unidade { get; set; }
    public TipoProduto Tipo { get; set; }
    public SituacaoProduto Situacao { get; set; }
    public FormatoProduto Formato { get; set; }
    public string? DescricaoCurta { get; set; }
    public string? Gtin { get; set; }
    public string? GtinEmbalagem { get; set; }
    public decimal PesoLiquido { get; set; }
    public decimal PesoBruto { get; set; }
    public int Volumes { get; set; }
    public decimal ItensPorCaixa { get; set; }
    public TipoProducao TipoProducao { get; set; }
    public CondicaoProduto Condicao { get; set; }
    public bool FreteGratis { get; set; }
    public string? Marca { get; set; }
    public string? DescricaoComplementar { get; set; }
    public string? LinkExterno { get; set; }
    public string? Observacoes { get; set; }
    public string? DescricaoEmbalagemDiscreta { get; set; }
    public string? ImagemURL { get; set; }
    public string? DataValidade { get; set; }

    // Relacionamentos
    public ProdutoEstoque? Estoque { get; set; }
    public ProdutoCategoria? Categoria { get; set; }
    public ProdutoDimensoes? Dimensoes { get; set; }
    public ProdutoTributacao? Tributacao { get; set; }
    public ProdutoFornecedor? Fornecedor { get; set; }
    public ProdutoMidia? Midia { get; set; }
    public ProdutoLinhaProduto? LinhaProduto { get; set; }
    public ProdutoEstrutura? Estrutura { get; set; }
    public ProdutoVariacao? Variacao { get; set; }
    public List<Produto>? Variacoes { get; set; }

    /// <summary>
    /// Verifica se o produto está ativo
    /// </summary>
    public bool EstaAtivo => Situacao == SituacaoProduto.Ativo;

    /// <summary>
    /// Verifica se o produto possui variações
    /// </summary>
    public bool PossuiVariacoes => Formato == FormatoProduto.ComVariacoes && Variacoes is { Count: > 0 };

    /// <summary>
    /// Verifica se o produto é uma composição
    /// </summary>
    public bool EhComposicao => Formato == FormatoProduto.ComComposicao;
}

/// <summary>
/// Informações de estoque do produto
/// </summary>
public class ProdutoEstoque
{
    public decimal Minimo { get; set; }
    public decimal Maximo { get; set; }
    public int Crossdocking { get; set; }
    public string? Localizacao { get; set; }
    public decimal SaldoVirtualTotal { get; set; }
}

/// <summary>
/// Categoria do produto
/// </summary>
public class ProdutoCategoria
{
    public long Id { get; set; }
    public string? Nome { get; set; }
}

/// <summary>
/// Dimensões do produto
/// </summary>
public class ProdutoDimensoes
{
    public decimal Largura { get; set; }
    public decimal Altura { get; set; }
    public decimal Profundidade { get; set; }
    /// <summary>
    /// Unidade de medida: 0=Metros, 1=Centímetros, 2=Milímetros
    /// </summary>
    public int UnidadeMedida { get; set; }
}

/// <summary>
/// Tributação do produto
/// </summary>
public class ProdutoTributacao
{
    public int Origem { get; set; }
    public string? Ncm { get; set; }
    public string? Cest { get; set; }
    public string? CodigoListaServicos { get; set; }
    public string? SpedTipoItem { get; set; }
    public decimal PercentualTributos { get; set; }
    public decimal ValorBaseStRetencao { get; set; }
    public decimal ValorStRetencao { get; set; }
    public decimal ValorICMSSubstituto { get; set; }
    public string? CodigoExcecaoTipi { get; set; }
    public string? ClasseEnquadramentoIpi { get; set; }
    public decimal ValorIpiFixo { get; set; }
    public string? CodigoSeloIpi { get; set; }
    public decimal ValorPisFixo { get; set; }
    public decimal ValorCofinsFixo { get; set; }
    public string? CodigoAnp { get; set; }
    public string? DescricaoAnp { get; set; }
    public decimal PercentualGLP { get; set; }
    public decimal PercentualGasNacional { get; set; }
    public decimal PercentualGasImportado { get; set; }
    public decimal ValorPartida { get; set; }
    public int TipiProveniencia { get; set; }
}

/// <summary>
/// Fornecedor do produto
/// </summary>
public class ProdutoFornecedor
{
    public long Id { get; set; }
    public ContatoSimples? Contato { get; set; }
    public string? Codigo { get; set; }
    public decimal PrecoCusto { get; set; }
    public decimal PrecoCompra { get; set; }
}

public class ContatoSimples
{
    public long Id { get; set; }
    public string? Nome { get; set; }
}

/// <summary>
/// Mídia do produto
/// </summary>
public class ProdutoMidia
{
    public ProdutoVideo? Video { get; set; }
    public ProdutoImagens? Imagens { get; set; }
}

public class ProdutoVideo
{
    public string? Url { get; set; }
}

public class ProdutoImagens
{
    public List<ImagemExterna>? Externas { get; set; }
    public List<ImagemInterna>? Internas { get; set; }
}

public class ImagemExterna
{
    public string? Link { get; set; }
}

public class ImagemInterna
{
    public string? Link { get; set; }
    public string? Validade { get; set; }
}

/// <summary>
/// Linha de produto
/// </summary>
public class ProdutoLinhaProduto
{
    public long Id { get; set; }
    public string? Nome { get; set; }
}

/// <summary>
/// Estrutura (composição) do produto
/// </summary>
public class ProdutoEstrutura
{
    public TipoEstoqueEstrutura TipoEstoque { get; set; }
    public LancamentoEstoque LancamentoEstoque { get; set; }
    public List<ProdutoComponente>? Componentes { get; set; }
}

/// <summary>
/// Componente de um produto composto
/// </summary>
public class ProdutoComponente
{
    public long ProdutoId { get; set; }
    public decimal Quantidade { get; set; }
}

/// <summary>
/// Variação do produto
/// </summary>
public class ProdutoVariacao
{
    public string? Nome { get; set; }
    public int Ordem { get; set; }
    public ProdutoPai? ProdutoPai { get; set; }
}

public class ProdutoPai
{
    public long Id { get; set; }
    public int ClpieTipo { get; set; }
}
