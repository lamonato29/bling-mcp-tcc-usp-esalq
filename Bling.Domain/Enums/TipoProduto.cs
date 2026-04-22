namespace Bling.Domain.Enums;

/// <summary>
/// Tipo do produto no Bling
/// </summary>
public enum TipoProduto
{
    Produto,
    Servico,
    ServicoOutros
}

/// <summary>
/// Formato do produto no Bling
/// </summary>
public enum FormatoProduto
{
    Simples,
    ComVariacoes,
    ComComposicao
}

/// <summary>
/// Situação do produto no Bling
/// </summary>
public enum SituacaoProduto
{
    Ativo,
    Inativo
}

/// <summary>
/// Condição do produto
/// </summary>
public enum CondicaoProduto
{
    NaoEspecificado = 0,
    Novo = 1,
    Usado = 2
}

/// <summary>
/// Tipo de produção do produto
/// </summary>
public enum TipoProducao
{
    Propria,
    Terceiros
}
