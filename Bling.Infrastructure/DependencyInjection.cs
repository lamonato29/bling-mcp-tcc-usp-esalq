using Bling.Application;
using Bling.Application.Interfaces;
using Bling.Infrastructure.Auth;
using Bling.Infrastructure.BackgroundTasks;
using Bling.Infrastructure.BlingApi;
using Bling.Infrastructure.BlingApi.Repositories;
using Bling.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Bling.Infrastructure;

/// <summary>
/// Extensão para registrar serviços de infraestrutura no DI container.
/// Registra apenas adapters/implementations — Use Cases são registrados pela Application.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra Infrastructure services (adapters) + chama AddBlingApplication() para Use Cases
    /// </summary>
    public static IServiceCollection AddBlingInfrastructure(
        this IServiceCollection services,
        OAuthConfiguration oauthConfig,
        string? postgresConnectionString = null)
    {
        // --- Infrastructure Services (implementações dos ports) ---

        // Token Storage + Cache + BackgroundTasks
        if (!string.IsNullOrEmpty(postgresConnectionString))
        {
            services.AddSingleton<ITokenStorage>(sp => new PostgresTokenStorage(postgresConnectionString));
            services.AddSingleton<ICacheService>(sp => new PostgresCacheService(postgresConnectionString));
            services.AddSingleton<IBackgroundTaskService>(sp => new PostgresBackgroundTaskService(postgresConnectionString));
        }
        else
        {
            var tokenPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "bling-mcp", "bling_tokens.json");
            services.AddSingleton<ITokenStorage>(sp => new FileTokenStorage(tokenPath));
            services.AddSingleton<ICacheService, InMemoryCacheService>();
            services.AddSingleton<IBackgroundTaskService, InMemoryBackgroundTaskService>();
        }

        // Auth
        services.AddSingleton<OAuthConfiguration>(oauthConfig);
        services.AddSingleton<IAuthService, BlingOAuthService>();

        // API Gateway (internal to Infrastructure)
        services.AddSingleton<BlingApiGateway>();
        services.AddSingleton<IBlingApiGateway>(sp => sp.GetRequiredService<BlingApiGateway>());

        // --- Repository Implementations ---
        services.AddTransient<IPedidoRepository, BlingPedidoRepository>();
        services.AddTransient<IProdutoRepository, BlingProdutoRepository>();
        services.AddTransient<IContatoRepository, BlingContatoRepository>();
        services.AddTransient<IContaRepository, BlingContaRepository>();
        services.AddTransient<IAuxiliarRepository, BlingAuxiliarRepository>();

        // --- Application Services (Use Cases) ---
        services.AddBlingApplication();

        return services;
    }
}
