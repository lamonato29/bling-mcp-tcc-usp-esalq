using System.Text.Json.Nodes;
using Bling.Application.DTOs;
using Bling.Application.Interfaces;
using Bling.Domain.Entities;

namespace Bling.Infrastructure.BlingApi.Repositories;

/// <summary>
/// Implementação do repositório de contas (pagar/receber) usando a API Bling v3.
/// </summary>
public class BlingContaRepository : IContaRepository
{
    private readonly IBlingApiGateway _api;

    public BlingContaRepository(IBlingApiGateway api)
    {
        _api = api;
    }

    public async Task<List<ContaReceber>> ListarReceberAsync(ContaFiltrosDto filtros)
    {
        var queryParams = BuildQueryParams(filtros);
        var response = await _api.GetAsync("/contas/receber", queryParams);
        if (response == null) return new List<ContaReceber>();

        var dataArray = response["data"]?.AsArray();
        if (dataArray == null) return new List<ContaReceber>();

        var contas = new List<ContaReceber>();
        foreach (var item in dataArray)
        {
            if (item == null) continue;
            var conta = new ContaReceber
            {
                Id = item["id"]?.GetValue<long>() ?? 0,
                Situacao = item["situacao"]?.GetValue<int>() ?? 0,
                Valor = item["valor"]?.GetValue<decimal>() ?? 0,
                NomeContato = item["contato"]?["nome"]?.GetValue<string>(),
                IdContato = item["contato"]?["id"]?.GetValue<long>(),
            };

            if (DateTime.TryParse(item["vencimento"]?.GetValue<string>(), out var venc))
                conta.Vencimento = venc;

            contas.Add(conta);
        }

        return contas;
    }

    public async Task<List<ContaPagar>> ListarPagarAsync(ContaFiltrosDto filtros)
    {
        var queryParams = BuildQueryParams(filtros);
        var response = await _api.GetAsync("/contas/pagar", queryParams);
        if (response == null) return new List<ContaPagar>();

        var dataArray = response["data"]?.AsArray();
        if (dataArray == null) return new List<ContaPagar>();

        var contas = new List<ContaPagar>();
        foreach (var item in dataArray)
        {
            if (item == null) continue;
            var conta = new ContaPagar
            {
                Id = item["id"]?.GetValue<long>() ?? 0,
                Situacao = item["situacao"]?.GetValue<int>() ?? 0,
                Valor = item["valor"]?.GetValue<decimal>() ?? 0,
                NomeContato = item["contato"]?["nome"]?.GetValue<string>(),
                IdContato = item["contato"]?["id"]?.GetValue<long>(),
            };

            if (DateTime.TryParse(item["vencimento"]?.GetValue<string>(), out var venc))
                conta.Vencimento = venc;

            contas.Add(conta);
        }

        return contas;
    }

    private static Dictionary<string, string> BuildQueryParams(ContaFiltrosDto filtros)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "pagina", filtros.Pagina.ToString() },
            { "limite", Math.Min(filtros.Limite, 100).ToString() }
        };

        if (filtros.DataEmissaoInicial.HasValue)
            queryParams["dataEmissaoInicial"] = filtros.DataEmissaoInicial.Value.ToString("yyyy-MM-dd");
        if (filtros.DataEmissaoFinal.HasValue)
            queryParams["dataEmissaoFinal"] = filtros.DataEmissaoFinal.Value.ToString("yyyy-MM-dd");
        if (filtros.DataVencimentoInicial.HasValue)
            queryParams["dataVencimentoInicial"] = filtros.DataVencimentoInicial.Value.ToString("yyyy-MM-dd");
        if (filtros.DataVencimentoFinal.HasValue)
            queryParams["dataVencimentoFinal"] = filtros.DataVencimentoFinal.Value.ToString("yyyy-MM-dd");
        if (!string.IsNullOrEmpty(filtros.Situacao))
            queryParams["situacao"] = filtros.Situacao;

        return queryParams;
    }
}
