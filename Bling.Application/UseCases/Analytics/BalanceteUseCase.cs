using Bling.Application.DTOs;
using Bling.Application.UseCases.Financeiro;

namespace Bling.Application.UseCases.Analytics;

/// <summary>
/// Use Case: Balancete simplificado (Receber vs Pagar)
/// </summary>
public class BalanceteUseCase
{
    private readonly ListarContasReceberUseCase _contasReceberUseCase;
    private readonly ListarContasPagarUseCase _contasPagarUseCase;

    public BalanceteUseCase(
        ListarContasReceberUseCase contasReceberUseCase,
        ListarContasPagarUseCase contasPagarUseCase)
    {
        _contasReceberUseCase = contasReceberUseCase;
        _contasPagarUseCase = contasPagarUseCase;
    }

    public async Task<BalanceteDto> ExecuteAsync(DateTime dataInicial, DateTime dataFinal)
    {
        var filtrosReceber = new ContaFiltrosDto
        {
            DataVencimentoInicial = dataInicial,
            DataVencimentoFinal = dataFinal,
            Limite = 100
        };
        var filtrosPagar = new ContaFiltrosDto
        {
            DataVencimentoInicial = dataInicial,
            DataVencimentoFinal = dataFinal,
            Limite = 100
        };

        var taskReceber = _contasReceberUseCase.ExecuteAsync(filtrosReceber);
        var taskPagar = _contasPagarUseCase.ExecuteAsync(filtrosPagar);

        await Task.WhenAll(taskReceber, taskPagar);

        var receber = taskReceber.Result;
        var pagar = taskPagar.Result;

        var totalReceber = receber.Sum(c => c.Valor);
        var totalRecebido = receber.Where(c => c.Situacao == 2).Sum(c => c.Valor);
        var totalReceberAberto = receber.Where(c => c.Situacao == 1).Sum(c => c.Valor);

        var totalPagar = pagar.Sum(c => c.Valor);
        var totalPago = pagar.Where(c => c.Situacao == 2).Sum(c => c.Valor);
        var totalPagarAberto = pagar.Where(c => c.Situacao == 1).Sum(c => c.Valor);

        return new BalanceteDto
        {
            Periodo = $"{dataInicial:dd/MM/yyyy} a {dataFinal:dd/MM/yyyy}",
            Receitas = new BalanceteResumoDto
            {
                Total = totalReceber,
                Realizado = totalRecebido,
                Previsto = totalReceberAberto
            },
            Despesas = new BalanceteResumoDto
            {
                Total = totalPagar,
                Realizado = totalPago,
                Previsto = totalPagarAberto
            },
            Saldo = new BalanceteSaldoDto
            {
                Geral = totalReceber - totalPagar,
                Realizado = totalRecebido - totalPago
            },
            QtdTitulosReceber = receber.Count,
            QtdTitulosPagar = pagar.Count
        };
    }
}
