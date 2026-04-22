using Bling.Domain.Enums;

namespace Bling.Infrastructure.BlingApi;

/// <summary>
/// Mapeamento de valores da API Bling ↔ tipos de domínio.
/// Centraliza toda conversão que antes ficava nos enums do Domain.
/// </summary>
public static class BlingApiMapper
{
    #region TipoPessoa

    public static string ToApiValue(TipoPessoa tipo) => tipo switch
    {
        TipoPessoa.Fisica => "F",
        TipoPessoa.Juridica => "J",
        TipoPessoa.Estrangeira => "E",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };

    public static TipoPessoa TipoPessoaFromApi(string? value) => value switch
    {
        "F" => TipoPessoa.Fisica,
        "J" => TipoPessoa.Juridica,
        "E" => TipoPessoa.Estrangeira,
        _ => TipoPessoa.Fisica
    };

    #endregion

    #region TipoProduto

    public static string ToApiValue(TipoProduto tipo) => tipo switch
    {
        TipoProduto.Produto => "P",
        TipoProduto.Servico => "S",
        TipoProduto.ServicoOutros => "N",
        _ => "P"
    };

    public static TipoProduto TipoProdutoFromApi(string? value) => value switch
    {
        "P" => TipoProduto.Produto,
        "S" => TipoProduto.Servico,
        "N" => TipoProduto.ServicoOutros,
        _ => TipoProduto.Produto
    };

    #endregion

    #region FormatoProduto

    public static string ToApiValue(FormatoProduto formato) => formato switch
    {
        FormatoProduto.Simples => "S",
        FormatoProduto.ComVariacoes => "V",
        FormatoProduto.ComComposicao => "E",
        _ => "S"
    };

    public static FormatoProduto FormatoProdutoFromApi(string? value) => value switch
    {
        "S" => FormatoProduto.Simples,
        "V" => FormatoProduto.ComVariacoes,
        "E" => FormatoProduto.ComComposicao,
        _ => FormatoProduto.Simples
    };

    #endregion

    #region SituacaoProduto

    public static string ToApiValue(SituacaoProduto situacao) => situacao switch
    {
        SituacaoProduto.Ativo => "A",
        SituacaoProduto.Inativo => "I",
        _ => "A"
    };

    public static SituacaoProduto SituacaoProdutoFromApi(string? value) => value switch
    {
        "A" => SituacaoProduto.Ativo,
        "I" => SituacaoProduto.Inativo,
        _ => SituacaoProduto.Ativo
    };

    #endregion

    #region TipoFrete

    public static TipoFrete TipoFreteFromApi(int value) => value switch
    {
        0 => TipoFrete.CIF,
        1 => TipoFrete.FOB,
        2 => TipoFrete.Terceiros,
        3 => TipoFrete.ProprioRemetente,
        4 => TipoFrete.ProprioDestinatario,
        9 => TipoFrete.SemTransporte,
        _ => TipoFrete.SemTransporte
    };

    #endregion

    #region TipoProducao

    public static string ToApiValue(TipoProducao tipo) => tipo switch
    {
        TipoProducao.Propria => "P",
        TipoProducao.Terceiros => "T",
        _ => "P"
    };

    public static TipoProducao TipoProducaoFromApi(string? value) => value switch
    {
        "P" => TipoProducao.Propria,
        "T" => TipoProducao.Terceiros,
        _ => TipoProducao.Propria
    };

    #endregion

    #region OperacaoEstoque

    public static string ToApiValue(OperacaoEstoque op) => op switch
    {
        OperacaoEstoque.Entrada => "E",
        OperacaoEstoque.Saida => "S",
        _ => "E"
    };

    public static OperacaoEstoque OperacaoEstoqueFromApi(string? value) => value switch
    {
        "E" => OperacaoEstoque.Entrada,
        "S" => OperacaoEstoque.Saida,
        _ => OperacaoEstoque.Entrada
    };

    #endregion

    #region TipoEstoqueEstrutura

    public static string ToApiValue(TipoEstoqueEstrutura tipo) => tipo switch
    {
        TipoEstoqueEstrutura.Fisico => "F",
        TipoEstoqueEstrutura.Virtual => "V",
        _ => "F"
    };

    public static TipoEstoqueEstrutura TipoEstoqueEstruturaFromApi(string? value) => value switch
    {
        "F" => TipoEstoqueEstrutura.Fisico,
        "V" => TipoEstoqueEstrutura.Virtual,
        _ => TipoEstoqueEstrutura.Fisico
    };

    #endregion

    #region LancamentoEstoque

    public static string ToApiValue(LancamentoEstoque tipo) => tipo switch
    {
        LancamentoEstoque.ProdutoEComponente => "A",
        LancamentoEstoque.Componente => "M",
        LancamentoEstoque.Produto => "P",
        _ => "A"
    };

    public static LancamentoEstoque LancamentoEstoqueFromApi(string? value) => value switch
    {
        "A" => LancamentoEstoque.ProdutoEComponente,
        "M" => LancamentoEstoque.Componente,
        "P" => LancamentoEstoque.Produto,
        _ => LancamentoEstoque.ProdutoEComponente
    };

    #endregion
}
