namespace Bling.Domain.Entities;

/// <summary>
/// Nota fiscal do Bling
/// </summary>
public class NotaFiscal : Entity
{
    public int Tipo { get; set; }
    public string? Numero { get; set; }
    public string? Serie { get; set; }
    public DateTime DataEmissao { get; set; }
    public DateTime? DataSaida { get; set; }
    public int Situacao { get; set; }
    public NotaFiscalContato? Contato { get; set; }
    public NotaFiscalNaturezaOperacao? NaturezaOperacao { get; set; }
    public string? ChaveAcesso { get; set; }
    public string? Xml { get; set; }
    public string? LinkDanfe { get; set; }
    public string? LinkPdf { get; set; }
    public decimal ValorNota { get; set; }
}

public class NotaFiscalContato
{
    public long Id { get; set; }
    public string? Nome { get; set; }
    public string? NumeroDocumento { get; set; }
}

public class NotaFiscalNaturezaOperacao
{
    public long Id { get; set; }
}
