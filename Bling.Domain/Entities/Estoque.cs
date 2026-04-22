namespace Bling.Domain.Entities;

/// <summary>
/// Registro de estoque de um produto em um depósito
/// </summary>
public class EstoqueItem : Entity
{
    public EstoqueProduto? Produto { get; set; }
    public Deposito? Deposito { get; set; }
    public decimal SaldoFisico { get; set; }
    public decimal SaldoVirtual { get; set; }
    public decimal SaldoFisicoTotal { get; set; }
    public decimal SaldoVirtualTotal { get; set; }
}

/// <summary>
/// Produto resumido para contexto de estoque
/// </summary>
public class EstoqueProduto
{
    public long Id { get; set; }
    public string? Codigo { get; set; }
    public string? Nome { get; set; }
}

/// <summary>
/// Depósito de estoque
/// </summary>
public class Deposito : Entity
{
    public string? Descricao { get; set; }
    public int Situacao { get; set; }
    public bool Padrao { get; set; }
    public bool DesconsiderarSaldo { get; set; }

    public bool EstaAtivo => Situacao == 1;
}

/// <summary>
/// Movimentação de estoque
/// </summary>
public class MovimentacaoEstoque
{
    public EstoqueProduto? Produto { get; set; }
    public Deposito? Deposito { get; set; }
    public string? Operacao { get; set; }
    public decimal Quantidade { get; set; }
    public string? Observacoes { get; set; }
}
