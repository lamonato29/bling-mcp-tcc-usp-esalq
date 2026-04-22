namespace Bling.Domain.Enums;

/// <summary>
/// Tipo de pessoa (física, jurídica, estrangeira)
/// </summary>
public enum TipoPessoa
{
    Fisica,
    Juridica,
    Estrangeira
}

public static class TipoPessoaExtensions
{
    public static string ToDescricao(this TipoPessoa tipo) => tipo switch
    {
        TipoPessoa.Fisica => "Física",
        TipoPessoa.Juridica => "Jurídica",
        TipoPessoa.Estrangeira => "Estrangeira",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };
}
