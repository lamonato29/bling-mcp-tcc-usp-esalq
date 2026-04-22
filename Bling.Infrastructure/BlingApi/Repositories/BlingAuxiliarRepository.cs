using System.Text.Json.Nodes;
using Bling.Application.Interfaces;
using Bling.Domain.Entities;

namespace Bling.Infrastructure.BlingApi.Repositories;

/// <summary>
/// Implementação do repositório de dados auxiliares usando a API Bling v3.
/// </summary>
public class BlingAuxiliarRepository : IAuxiliarRepository
{
    private readonly IBlingApiGateway _api;

    public BlingAuxiliarRepository(IBlingApiGateway api)
    {
        _api = api;
    }

    public async Task<List<CanalVenda>> ListarCanaisVendaAsync()
    {
        var response = await _api.GetAsync("/canais-venda");
        if (response == null) return new List<CanalVenda>();

        var dataArray = response["data"]?.AsArray();
        if (dataArray == null) return new List<CanalVenda>();

        var canais = new List<CanalVenda>();
        foreach (var item in dataArray)
        {
            if (item == null) continue;
            canais.Add(new CanalVenda
            {
                Id = item["id"]?.GetValue<long>() ?? 0,
                Descricao = item["descricao"]?.GetValue<string>(),
                Tipo = item["tipo"]?.GetValue<string>(),
                Situacao = item["situacao"]?.GetValue<int>() ?? 0
            });
        }

        return canais;
    }

    public async Task<List<SituacaoCustomizada>> ListarSituacoesAsync(long? idModulo = null)
    {
        var endpoint = idModulo.HasValue
            ? $"/situacoes/modulos/{idModulo}"
            : "/situacoes/modulos";

        var response = await _api.GetAsync(endpoint);
        if (response == null) return new List<SituacaoCustomizada>();

        var dataArray = response["data"]?.AsArray();
        if (dataArray == null) return new List<SituacaoCustomizada>();

        var situacoes = new List<SituacaoCustomizada>();
        foreach (var item in dataArray)
        {
            if (item == null) continue;
            situacoes.Add(new SituacaoCustomizada
            {
                Id = item["id"]?.GetValue<long>() ?? 0,
                Nome = item["nome"]?.GetValue<string>(),
                Cor = item["cor"]?.GetValue<string>()
            });
        }

        return situacoes;
    }

    public async Task<List<Deposito>> ListarDepositosAsync()
    {
        var response = await _api.GetAsync("/depositos");
        if (response == null) return new List<Deposito>();

        var dataArray = response["data"]?.AsArray();
        if (dataArray == null) return new List<Deposito>();

        var depositos = new List<Deposito>();
        foreach (var item in dataArray)
        {
            if (item == null) continue;
            depositos.Add(new Deposito
            {
                Id = item["id"]?.GetValue<long>() ?? 0,
                Descricao = item["descricao"]?.GetValue<string>(),
                Situacao = item["situacao"]?.GetValue<int>() ?? 0,
                Padrao = item["padrao"]?.GetValue<bool>() ?? false,
                DesconsiderarSaldo = item["desconsiderarSaldo"]?.GetValue<bool>() ?? false
            });
        }

        return depositos;
    }

    public async Task<Dictionary<long, string>> ObterSituacoesDictAsync()
    {
        var response = await _api.GetAsync("/situacoes/modulos");
        var dict = new Dictionary<long, string>();

        if (response != null)
        {
            var dataArray = response["data"]?.AsArray();
            if (dataArray != null)
            {
                foreach (var item in dataArray)
                {
                    if (item == null) continue;
                    var id = item["id"]?.GetValue<long>() ?? 0;
                    var nome = item["nome"]?.GetValue<string>() ?? $"ID {id}";
                    dict[id] = nome;
                }
            }
        }

        return dict;
    }
}
