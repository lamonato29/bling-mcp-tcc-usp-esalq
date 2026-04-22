namespace Bling.Domain.Enums;

/// <summary>
/// Situação de uma conta a pagar ou receber
/// </summary>
public enum SituacaoConta
{
    EmAberto = 1,
    Recebido = 2,
    ParcialmenteRecebido = 3,
    Devolvido = 4,
    Cancelado = 5
}

/// <summary>
/// Status de execução de uma tarefa em background
/// </summary>
public enum StatusBackgroundTask
{
    Pendente,
    Executando,
    Concluida,
    Falhou,
    Cancelada
}

/// <summary>
/// Tipo de operação de estoque
/// </summary>
public enum OperacaoEstoque
{
    Entrada,
    Saida
}

/// <summary>
/// Tipo de estoque na estrutura (composição) do produto
/// </summary>
public enum TipoEstoqueEstrutura
{
    Fisico,
    Virtual
}

/// <summary>
/// Tipo de lançamento de estoque na estrutura (composição)
/// </summary>
public enum LancamentoEstoque
{
    ProdutoEComponente,
    Componente,
    Produto
}

public static class LancamentoEstoqueExtensions
{
    public static string ToDescricao(this LancamentoEstoque tipo) => tipo switch
    {
        LancamentoEstoque.ProdutoEComponente => "Produto e Componente",
        LancamentoEstoque.Componente => "Componente",
        LancamentoEstoque.Produto => "Produto",
        _ => "Desconhecido"
    };
}
