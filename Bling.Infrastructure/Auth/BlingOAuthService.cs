using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Bling.Application.Interfaces;

namespace Bling.Infrastructure.Auth;

/// <summary>
/// Configuração OAuth para o Bling
/// </summary>
public class OAuthConfiguration
{
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public string RedirectUri { get; set; } = "http://localhost:8087/callback/";
    public string ApiBaseUrl { get; set; } = "https://www.bling.com.br/Api/v3";
    public int HttpTimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// Implementação do serviço de autenticação OAuth 2.0 para Bling.
/// Implementa IAuthService (port da Application).
/// </summary>
public class BlingOAuthService : IAuthService
{
    private const string AuthorizationEndpoint = "https://www.bling.com.br/Api/v3/oauth/authorize";
    private const string TokenEndpoint = "https://www.bling.com.br/Api/v3/oauth/token";

    private readonly OAuthConfiguration _config;
    private readonly ITokenStorage _tokenStorage;
    private readonly HttpClient _httpClient;

    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime TokenExpiresAt { get; private set; }

    public BlingOAuthService(OAuthConfiguration config, ITokenStorage tokenStorage)
    {
        _config = config;
        _tokenStorage = tokenStorage;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(_config.HttpTimeoutSeconds) };
    }

    public bool IsTokenValid()
    {
        if (string.IsNullOrEmpty(AccessToken)) return false;
        var buffer = TimeSpan.FromMinutes(5);
        if (TokenExpiresAt <= DateTime.MinValue + buffer) return false;
        return DateTime.Now < TokenExpiresAt - buffer;
    }

    public async Task<bool> AuthorizeAsync()
    {
        try
        {
            var state = Guid.NewGuid().ToString("N");
            var authorizationCode = await ListenForAuthorizationCodeAsync(state);
            if (!string.IsNullOrEmpty(authorizationCode))
                return await ExchangeCodeForTokensAsync(authorizationCode);
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro durante a autorização: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        if (string.IsNullOrEmpty(RefreshToken)) return false;

        try
        {
            var credentials = $"{_config.ClientId}:{_config.ClientSecret}";
            var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));

            using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", RefreshToken)
            });

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonNode.Parse(content);
                if (json == null) return false;

                AccessToken = json["access_token"]?.GetValue<string>();
                RefreshToken = json["refresh_token"]?.GetValue<string>() ?? RefreshToken;
                if (int.TryParse(json["expires_in"]?.ToString(), out int expiresIn))
                    TokenExpiresAt = DateTime.Now.AddSeconds(expiresIn);

                await _tokenStorage.StoreTokensAsync(AccessToken!, RefreshToken!, TokenExpiresAt);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro ao renovar token: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> EnsureValidTokenAsync()
    {
        if (IsTokenValid()) return true;

        if (!string.IsNullOrEmpty(RefreshToken))
        {
            if (await RefreshTokenAsync()) return true;
            Console.Error.WriteLine("[Auth] Refresh token expirado. Iniciando nova autenticação...");
            ClearTokens();
            await _tokenStorage.ClearTokensAsync();
        }

        Console.Error.WriteLine("[Auth] Token não disponível. Abrindo navegador para autenticação...");
        return await AuthorizeAsync();
    }

    public void SetTokens(string accessToken, string refreshToken, DateTime expiresAt)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        TokenExpiresAt = expiresAt;
    }

    public void ClearTokens()
    {
        AccessToken = null;
        RefreshToken = null;
        TokenExpiresAt = DateTime.MinValue;
    }

    public async Task LoadTokensAsync()
    {
        var (accessToken, refreshToken, expiresAt) = await _tokenStorage.RetrieveTokensAsync();
        if (!string.IsNullOrEmpty(accessToken))
            SetTokens(accessToken, refreshToken!, expiresAt);
    }

    private async Task<string?> ListenForAuthorizationCodeAsync(string state)
    {
        using var listener = new HttpListener();
        listener.Prefixes.Add(_config.RedirectUri);

        try { listener.Start(); }
        catch (HttpListenerException ex)
        {
            Debug.WriteLine($"Erro ao iniciar servidor local: {ex.Message}");
            return null;
        }

        var authorizationUrl = $"{AuthorizationEndpoint}?response_type=code&client_id={_config.ClientId}&state={state}";
        Console.Error.WriteLine($"[Auth] Abrindo navegador: {authorizationUrl}");

        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
            Process.Start(new ProcessStartInfo(authorizationUrl) { UseShellExecute = true });
        else
            Console.Error.WriteLine("[Auth] Ambiente headless — copie a URL acima.");

        var getContextTask = listener.GetContextAsync();
        var timeoutTask = Task.Delay(TimeSpan.FromMinutes(2));
        var completedTask = await Task.WhenAny(getContextTask, timeoutTask);

        if (completedTask == timeoutTask)
        {
            listener.Stop();
            Console.Error.WriteLine("[Auth] Timeout aguardando autenticação.");
            return null;
        }

        var context = await getContextTask;
        var code = context.Request.QueryString.Get("code");
        var incomingState = context.Request.QueryString.Get("state");
        var error = context.Request.QueryString.Get("error");

        string responseString = !string.IsNullOrEmpty(error)
            ? $"<html><body><h2>Erro na autorização</h2><p>{error}</p></body></html>"
            : !string.IsNullOrEmpty(code)
                ? "<html><body><h2>Autorização concluída!</h2><p>Pode fechar esta janela.</p></body></html>"
                : "<html><body><h2>Erro</h2><p>Código não recebido.</p></body></html>";

        var buffer = Encoding.UTF8.GetBytes(responseString);
        context.Response.ContentLength64 = buffer.Length;
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.OutputStream.WriteAsync(buffer);
        context.Response.OutputStream.Close();
        listener.Stop();

        if (incomingState != state) { Debug.WriteLine("CSRF error"); return null; }
        if (!string.IsNullOrEmpty(error)) { Debug.WriteLine($"Auth denied: {error}"); return null; }
        return code;
    }

    private async Task<bool> ExchangeCodeForTokensAsync(string code)
    {
        try
        {
            var credentials = $"{_config.ClientId}:{_config.ClientSecret}";
            var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));

            using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code)
            });

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonNode.Parse(content);
                if (json == null) return false;

                AccessToken = json["access_token"]?.GetValue<string>();
                RefreshToken = json["refresh_token"]?.GetValue<string>();
                if (int.TryParse(json["expires_in"]?.ToString(), out int expiresIn))
                    TokenExpiresAt = DateTime.Now.AddSeconds(expiresIn);

                await _tokenStorage.StoreTokensAsync(AccessToken!, RefreshToken!, TokenExpiresAt);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro ao trocar código por tokens: {ex.Message}");
            return false;
        }
    }
}
