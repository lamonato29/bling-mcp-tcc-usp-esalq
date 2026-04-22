using System.Text.Json.Nodes;
using Bling.Application.DTOs;
using Bling.Application.Interfaces;
using Bling.Domain.Entities;

namespace Bling.Infrastructure.BlingApi.Repositories;

/// <summary>
/// Implementação do repositório de pedidos usando a API Bling v3.
/// Encapsula endpoints, paginação e JSON parsing.
/// </summary>
public class BlingPedidoRepository : IPedidoRepository
{
    private readonly IBlingApiGateway _api;

    public BlingPedidoRepository(IBlingApiGateway api)
    {
        _api = api;
    }

    public async Task<List<Pedido>> ListarAsync(PedidoFiltrosDto filtros, int delayMs = 400, int maxPaginas = int.MaxValue, Action<int>? onPage = null)
    {
        var queryParams = BuildQueryParams(filtros);
        var allPedidos = new List<Pedido>();
        var pagina = 1;

        while (pagina <= maxPaginas)
        {
            queryParams["pagina"] = pagina.ToString();
            queryParams["limite"] = Math.Min(filtros.Limite, 100).ToString();

            var response = await _api.GetAsync("/pedidos/vendas", queryParams);
            if (response == null) break;

            var dataArray = response["data"]?.AsArray();
            if (dataArray == null || dataArray.Count == 0) break;

            foreach (var item in dataArray)
            {
                if (item == null) continue;
                allPedidos.Add(MapPedidoResumido(item));
            }

            onPage?.Invoke(allPedidos.Count);

            if (dataArray.Count < 100) break;
            pagina++;
            if (delayMs > 0) await Task.Delay(delayMs);
        }

        return allPedidos;
    }

    public async Task<Pedido?> ObterPorIdAsync(long id)
    {
        var response = await _api.GetAsync($"/pedidos/vendas/{id}");
        if (response == null) return null;

        var data = response["data"];
        if (data == null) return null;

        return MapPedidoDetalhado(data);
    }

    public async Task<List<Pedido>> BuscarPorNumeroAsync(int numero)
    {
        var response = await _api.GetAsync("/pedidos/vendas", new Dictionary<string, string>
        {
            { "numero", numero.ToString() },
            { "limite", "1" }
        });

        if (response == null) return new List<Pedido>();

        var dataArray = response["data"]?.AsArray();
        if (dataArray == null || dataArray.Count == 0) return new List<Pedido>();

        var result = new List<Pedido>();
        foreach (var item in dataArray)
        {
            if (item == null) continue;
            result.Add(MapPedidoResumido(item));
        }
        return result;
    }

    #region Query Params

    private static Dictionary<string, string> BuildQueryParams(PedidoFiltrosDto filtros)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "pagina", filtros.Pagina.ToString() },
            { "limite", Math.Min(filtros.Limite, 100).ToString() }
        };

        if (filtros.DataInicial.HasValue)
            queryParams["dataInicial"] = filtros.DataInicial.Value.ToString("yyyy-MM-dd");
        if (filtros.DataFinal.HasValue)
            queryParams["dataFinal"] = filtros.DataFinal.Value.ToString("yyyy-MM-dd");
        if (filtros.DataAlteracaoInicial.HasValue)
            queryParams["dataAlteracaoInicial"] = filtros.DataAlteracaoInicial.Value.ToString("yyyy-MM-dd HH:mm:ss");
        if (filtros.DataAlteracaoFinal.HasValue)
            queryParams["dataAlteracaoFinal"] = filtros.DataAlteracaoFinal.Value.ToString("yyyy-MM-dd HH:mm:ss");
        if (filtros.Numero.HasValue)
            queryParams["numero"] = filtros.Numero.Value.ToString();
        if (filtros.IdLoja.HasValue)
            queryParams["idLoja"] = filtros.IdLoja.Value.ToString();
        if (filtros.IdVendedor.HasValue)
            queryParams["idVendedor"] = filtros.IdVendedor.Value.ToString();
        if (filtros.IdContato.HasValue)
            queryParams["idContato"] = filtros.IdContato.Value.ToString();
        if (filtros.NumerosLojas is { Count: > 0 })
            queryParams["numerosLojas[]"] = string.Join(",", filtros.NumerosLojas);
        if (filtros.IdsSituacoes is { Count: > 0 })
            queryParams["idsSituacoes[]"] = string.Join(",", filtros.IdsSituacoes);

        return queryParams;
    }

    #endregion

    #region JSON Mapping

    private static Pedido MapPedidoResumido(JsonNode item)
    {
        var pedido = new Pedido
        {
            Id = item["id"]?.GetValue<long>() ?? 0,
            Numero = item["numero"]?.GetValue<int>() ?? 0,
            NumeroLoja = item["numeroLoja"]?.GetValue<string>(),
            Total = item["total"]?.GetValue<decimal>() ?? 0,
            TotalProdutos = item["totalProdutos"]?.GetValue<decimal>() ?? 0,
        };

        if (DateTime.TryParse(item["data"]?.GetValue<string>(), out var data))
            pedido.Data = data;

        var contato = item["contato"];
        if (contato != null)
        {
            pedido.Contato = new PedidoContato
            {
                Id = contato["id"]?.GetValue<long>() ?? 0,
                Nome = contato["nome"]?.GetValue<string>(),
                NumeroDocumento = contato["numeroDocumento"]?.GetValue<string>()
            };
        }

        var situacao = item["situacao"];
        if (situacao != null)
        {
            pedido.Situacao = new PedidoSituacao
            {
                Id = situacao["id"]?.GetValue<long>() ?? 0,
                Valor = situacao["valor"]?.GetValue<int>() ?? 0
            };
        }

        var loja = item["loja"];
        if (loja != null)
        {
            pedido.Loja = new CanalVenda
            {
                Id = loja["id"]?.GetValue<long>() ?? 0,
                Descricao = loja["descricao"]?.GetValue<string>(),
                Tipo = loja["tipo"]?.GetValue<string>()
            };
        }

        return pedido;
    }

    private static Pedido MapPedidoDetalhado(JsonNode data)
    {
        var pedido = new Pedido
        {
            Id = data["id"]?.GetValue<long>() ?? 0,
            Numero = data["numero"]?.GetValue<int>() ?? 0,
            NumeroLoja = data["numeroLoja"]?.GetValue<string>(),
            Total = data["total"]?.GetValue<decimal>() ?? 0,
            TotalProdutos = data["totalProdutos"]?.GetValue<decimal>() ?? 0,
            OutrasDespesas = data["outrasDespesas"]?.GetValue<decimal>() ?? 0,
            Observacoes = data["observacoes"]?.GetValue<string>(),
            ObservacoesInternas = data["observacoesInternas"]?.GetValue<string>(),
            NumeroPedidoCompra = data["numeroPedidoCompra"]?.GetValue<string>(),
        };

        if (DateTime.TryParse(data["data"]?.GetValue<string>(), out var dt))
            pedido.Data = dt;
        if (DateTime.TryParse(data["dataSaida"]?.GetValue<string>(), out var ds))
            pedido.DataSaida = ds;
        if (DateTime.TryParse(data["dataPrevista"]?.GetValue<string>(), out var dp))
            pedido.DataPrevista = dp;

        // Contato
        var contato = data["contato"];
        if (contato != null)
        {
            pedido.Contato = new PedidoContato
            {
                Id = contato["id"]?.GetValue<long>() ?? 0,
                Nome = contato["nome"]?.GetValue<string>(),
                NumeroDocumento = contato["numeroDocumento"]?.GetValue<string>()
            };
        }

        // Situacao
        var situacao = data["situacao"];
        if (situacao != null)
        {
            pedido.Situacao = new PedidoSituacao
            {
                Id = situacao["id"]?.GetValue<long>() ?? 0,
                Valor = situacao["valor"]?.GetValue<int>() ?? 0
            };
        }

        // Loja
        var loja = data["loja"];
        if (loja != null)
        {
            pedido.Loja = new CanalVenda
            {
                Id = loja["id"]?.GetValue<long>() ?? 0,
                Descricao = loja["descricao"]?.GetValue<string>(),
                Tipo = loja["tipo"]?.GetValue<string>()
            };
        }

        // Desconto
        var desconto = data["desconto"];
        if (desconto != null)
        {
            pedido.Desconto = new PedidoDesconto
            {
                Valor = desconto["valor"]?.GetValue<decimal>() ?? 0,
                Unidade = desconto["unidade"]?.GetValue<string>()
            };
        }

        // Vendedor
        var vendedor = data["vendedor"];
        if (vendedor != null)
            pedido.Vendedor = new PedidoVendedor { Id = vendedor["id"]?.GetValue<long>() ?? 0 };

        // NotaFiscal
        var nf = data["notaFiscal"];
        if (nf != null)
            pedido.NotaFiscal = new PedidoNotaFiscal { Id = nf["id"]?.GetValue<long>() ?? 0 };

        // Intermediador
        var inter = data["intermediador"];
        if (inter != null)
        {
            pedido.Intermediador = new PedidoIntermediador
            {
                Cnpj = inter["cnpj"]?.GetValue<string>(),
                NomeUsuario = inter["nomeUsuario"]?.GetValue<string>()
            };
        }

        // Taxas
        var taxas = data["taxas"];
        if (taxas != null)
        {
            pedido.Taxas = new PedidoTaxas
            {
                TaxaComissao = taxas["taxaComissao"]?.GetValue<decimal>() ?? 0,
                CustoFrete = taxas["custoFrete"]?.GetValue<decimal>() ?? 0,
                ValorBase = taxas["valorBase"]?.GetValue<decimal>() ?? 0
            };
        }

        // Itens
        var itensArray = data["itens"]?.AsArray();
        if (itensArray != null)
        {
            pedido.Itens = new List<PedidoItem>();
            foreach (var item in itensArray)
            {
                if (item == null) continue;
                var pedidoItem = new PedidoItem
                {
                    Id = item["id"]?.GetValue<long>() ?? 0,
                    Codigo = item["codigo"]?.GetValue<string>(),
                    Descricao = item["descricao"]?.GetValue<string>(),
                    Quantidade = item["quantidade"]?.GetValue<decimal>() ?? 0,
                    Valor = item["valor"]?.GetValue<decimal>() ?? 0,
                    Desconto = item["desconto"]?.GetValue<decimal>() ?? 0,
                    Unidade = item["unidade"]?.GetValue<string>(),
                };

                var produto = item["produto"];
                if (produto != null)
                    pedidoItem.Produto = new PedidoItemProduto { Id = produto["id"]?.GetValue<long>() ?? 0 };

                pedido.Itens.Add(pedidoItem);
            }
        }

        // Parcelas
        var parcelasArray = data["parcelas"]?.AsArray();
        if (parcelasArray != null)
        {
            pedido.Parcelas = new List<PedidoParcela>();
            foreach (var p in parcelasArray)
            {
                if (p == null) continue;
                var parcela = new PedidoParcela
                {
                    Id = p["id"]?.GetValue<long>() ?? 0,
                    DataVencimento = p["dataVencimento"]?.GetValue<string>(),
                    Valor = p["valor"]?.GetValue<decimal>() ?? 0,
                    Observacoes = p["observacoes"]?.GetValue<string>(),
                };

                var fp = p["formaPagamento"];
                if (fp != null)
                    parcela.FormaPagamento = new PedidoFormaPagamento { Id = fp["id"]?.GetValue<long>() ?? 0 };

                pedido.Parcelas.Add(parcela);
            }
        }

        // Transporte
        var transporte = data["transporte"];
        if (transporte != null)
        {
            pedido.Transporte = new PedidoTransporte
            {
                FretePorConta = BlingApiMapper.TipoFreteFromApi(transporte["fretePorConta"]?.GetValue<int>() ?? 9),
                Frete = transporte["frete"]?.GetValue<decimal>() ?? 0,
                QuantidadeVolumes = transporte["quantidadeVolumes"]?.GetValue<int>() ?? 0,
                PesoBruto = transporte["pesoBruto"]?.GetValue<decimal>() ?? 0,
                PrazoEntrega = transporte["prazoEntrega"]?.GetValue<int>() ?? 0
            };

            var tContato = transporte["contato"];
            if (tContato != null)
            {
                pedido.Transporte.Contato = new TransporteContato
                {
                    Id = tContato["id"]?.GetValue<long>() ?? 0,
                    Nome = tContato["nome"]?.GetValue<string>()
                };
            }

            var volumes = transporte["volumes"]?.AsArray();
            if (volumes != null)
            {
                pedido.Transporte.Volumes = new List<TransporteVolume>();
                foreach (var v in volumes)
                {
                    if (v == null) continue;
                    pedido.Transporte.Volumes.Add(new TransporteVolume
                    {
                        Id = v["id"]?.GetValue<long>() ?? 0,
                        Servico = v["servico"]?.GetValue<string>(),
                        CodigoRastreamento = v["codigoRastreamento"]?.GetValue<string>()
                    });
                }
            }
        }

        return pedido;
    }

    #endregion
}
