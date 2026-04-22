namespace Bling.Domain.Entities;

/// <summary>
/// Conta a pagar
/// </summary>
public class ContaPagar : Entity
{
    public int Situacao { get; set; }
    public DateTime? Vencimento { get; set; }
    public decimal Valor { get; set; }
    public long? IdContato { get; set; }
    public string? NomeContato { get; set; }
    public string? NumeroDocumento { get; set; }
    public DateTime? DataEmissao { get; set; }
    public DateTime? DataPagamento { get; set; }
    public decimal? ValorPago { get; set; }
    public string? Historico { get; set; }
    public long? IdCategoria { get; set; }
    public long? IdPortador { get; set; }
    public string? Ocorrencia { get; set; }
    public string? Observacoes { get; set; }

    public bool EstaEmAberto => Situacao == 1;
    public bool EstaPago => Situacao == 2;
}

/// <summary>
/// Conta a receber
/// </summary>
public class ContaReceber : Entity
{
    public int Situacao { get; set; }
    public DateTime? Vencimento { get; set; }
    public decimal Valor { get; set; }
    public long? IdContato { get; set; }
    public string? NomeContato { get; set; }
    public string? NumeroDocumento { get; set; }
    public DateTime? DataEmissao { get; set; }
    public DateTime? DataRecebimento { get; set; }
    public decimal? ValorRecebido { get; set; }
    public string? Historico { get; set; }
    public long? IdCategoria { get; set; }
    public long? IdPortador { get; set; }
    public long? IdVendedor { get; set; }
    public string? Ocorrencia { get; set; }
    public string? Observacoes { get; set; }
    public long? IdOrigem { get; set; }

    public bool EstaEmAberto => Situacao == 1;
    public bool EstaRecebido => Situacao == 2;
}

/// <summary>
/// Dados para baixa de conta
/// </summary>
public class BaixaConta
{
    public DateTime Data { get; set; } = DateTime.Today;
    public decimal? Valor { get; set; }
    public long? IdPortador { get; set; }
    public decimal? Tarifa { get; set; }
    public decimal? Juros { get; set; }
    public decimal? Desconto { get; set; }
    public decimal? Acrescimo { get; set; }
}
