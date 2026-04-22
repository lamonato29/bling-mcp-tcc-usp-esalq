using Bling.Application.UseCases.Analytics;
using Bling.Application.UseCases.Auxiliares;
using Bling.Application.UseCases.Cache;
using Bling.Application.UseCases.Contatos;
using Bling.Application.UseCases.Financeiro;
using Bling.Application.UseCases.Pedidos;
using Bling.Application.UseCases.Produtos;
using Microsoft.Extensions.DependencyInjection;

namespace Bling.Application;

/// <summary>
/// Extensão para registrar Use Cases da Application no DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddBlingApplication(this IServiceCollection services)
    {
        // Pedidos
        services.AddTransient<ListarPedidosUseCase>();
        services.AddTransient<ObterPedidoUseCase>();

        // Produtos
        services.AddTransient<ListarProdutosUseCase>();
        services.AddTransient<ObterProdutoUseCase>();

        // Contatos
        services.AddTransient<ListarContatosUseCase>();

        // Financeiro
        services.AddTransient<ListarContasReceberUseCase>();
        services.AddTransient<ListarContasPagarUseCase>();

        // Auxiliares
        services.AddTransient<ListarCanaisVendaUseCase>();
        services.AddTransient<ListarSituacoesUseCase>();
        services.AddTransient<ListarDepositosUseCase>();

        // Analytics
        services.AddTransient<PedidoAnalyticsUseCase>();
        services.AddTransient<ContatoAnalyticsUseCase>();
        services.AddTransient<BalanceteUseCase>();

        // Cache Management
        services.AddTransient<CacheManagementUseCase>();

        return services;
    }
}
