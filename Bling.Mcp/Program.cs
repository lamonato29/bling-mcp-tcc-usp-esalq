using Bling.Infrastructure;
using Bling.Infrastructure.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

LoadEnvFile(Path.Combine(AppContext.BaseDirectory, ".env"));
LoadEnvFile(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

var clientId = Environment.GetEnvironmentVariable("BLING_CLIENT_ID") ?? "";
var clientSecret = Environment.GetEnvironmentVariable("BLING_CLIENT_SECRET") ?? "";
var postgresConn = Environment.GetEnvironmentVariable("BLING_POSTGRES_CONNECTION");

if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
{
    Console.Error.WriteLine("[Bling.Mcp] ERRO: Defina BLING_CLIENT_ID e BLING_CLIENT_SECRET como variáveis de ambiente.");
    return;
}

var oauthConfig = new OAuthConfiguration
{
    ClientId = clientId,
    ClientSecret = clientSecret
};

builder.Services.AddBlingInfrastructure(oauthConfig, postgresConn);

builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new()
        {
            Name = "bling-mcp",
            Version = "1.0.0"
        };
    })
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
static void LoadEnvFile(string path)
{
    if (!File.Exists(path)) return;
    foreach (var line in File.ReadAllLines(path))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#')) continue;
        var idx = trimmed.IndexOf('=');
        if (idx <= 0) continue;
        var key = trimmed[..idx].Trim();
        var value = trimmed[(idx + 1)..].Trim();
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            Environment.SetEnvironmentVariable(key, value);
    }
}
