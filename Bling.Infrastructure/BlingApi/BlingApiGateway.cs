using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Bling.Application.Interfaces;

namespace Bling.Infrastructure.BlingApi;

/// <summary>
/// Cliente HTTP para a API do Bling v3.
/// Implementa IBlingApiGateway (port da Application).
/// </summary>
public class BlingApiGateway : IBlingApiGateway
{
    private const string BaseUrl = "https://www.bling.com.br/Api/v3";
    
    private readonly IAuthService _authService;
    private readonly HttpClient _httpClient;
    private readonly ApiTelemetryService _telemetry;

    public BlingApiGateway(IAuthService authService, int httpTimeoutSeconds = 30)
    {
        _authService = authService;
        _telemetry = new ApiTelemetryService();
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(httpTimeoutSeconds)
        };
    }

    public ApiTelemetryService Telemetry => _telemetry;

    public async Task<JsonObject?> GetAsync(string endpoint, Dictionary<string, string>? queryParams = null)
    {
        return await SendRequestAsync(HttpMethod.Get, endpoint, null, queryParams);
    }

    public async Task<JsonObject?> PostAsync(string endpoint, JsonObject body)
    {
        return await SendRequestAsync(HttpMethod.Post, endpoint, body);
    }

    public async Task<JsonObject?> PutAsync(string endpoint, JsonObject body)
    {
        return await SendRequestAsync(HttpMethod.Put, endpoint, body);
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        var result = await SendRequestAsync(HttpMethod.Delete, endpoint);
        return result != null;
    }

    public async Task<JsonObject?> PatchAsync(string endpoint, JsonObject? body = null)
    {
        return await SendRequestAsync(HttpMethod.Patch, endpoint, body);
    }

    private async Task<JsonObject?> SendRequestAsync(HttpMethod method, string endpoint, JsonObject? body = null, Dictionary<string, string>? queryParams = null)
    {
        await EnsureAuthenticatedAsync();
        
        var url = BuildUrl(endpoint, queryParams);
        
        using var request = new HttpRequestMessage(method, url);
        ConfigureRequest(request);
        
        if (body != null)
        {
            request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
        }

        return await SendRequestWithRetryAsync(request, method.Method, endpoint);
    }

    private async Task<JsonObject?> SendRequestWithRetryAsync(HttpRequestMessage request, string method, string endpoint, int maxRetries = 3)
    {
        var record = new ApiCallRecord
        {
            Endpoint = endpoint,
            Method = method,
            StartTime = DateTime.Now
        };

        try
        {
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                HttpResponseMessage response;
                if (attempt > 0)
                {
                    var cloned = await CloneHttpRequestMessageAsync(request);
                    response = await _httpClient.SendAsync(cloned);
                }
                else
                {
                    response = await _httpClient.SendAsync(request);
                }

                if ((int)response.StatusCode == 429 && attempt < maxRetries)
                {
                    var delay = Math.Pow(2, attempt + 1) * 1000;
                    Console.Error.WriteLine($"[API] Rate limited (429), waiting {delay}ms before retry {attempt + 1}/{maxRetries}");
                    await Task.Delay((int)delay);
                    continue;
                }

                var result = await HandleResponseAsync(response, endpoint);
                record.Success = result != null;
                record.StatusCode = (int)response.StatusCode;
                return result;
            }

            record.Success = false;
            return null;
        }
        catch (Exception ex)
        {
            record.Success = false;
            record.ErrorMessage = ex.Message;
            throw;
        }
        finally
        {
            record.EndTime = DateTime.Now;
            _telemetry.RecordCall(record);
        }
    }

    private async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage original)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri);
        
        if (original.Content != null)
        {
            var content = await original.Content.ReadAsStringAsync();
            clone.Content = new StringContent(content, Encoding.UTF8, original.Content.Headers.ContentType?.MediaType ?? "application/json");
        }
        
        foreach (var header in original.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        
        ConfigureRequest(clone);
        
        return clone;
    }

    private async Task EnsureAuthenticatedAsync()
    {
        if (!_authService.IsTokenValid())
        {
            await _authService.EnsureValidTokenAsync();
        }
    }

    private void ConfigureRequest(HttpRequestMessage request)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authService.AccessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private string BuildUrl(string endpoint, Dictionary<string, string>? queryParams)
    {
        var url = $"{BaseUrl}{endpoint}";
        
        if (queryParams != null && queryParams.Count > 0)
        {
            var query = string.Join("&", queryParams.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            url += $"?{query}";
        }
        
        return url;
    }

    private async Task<JsonObject?> HandleResponseAsync(HttpResponseMessage response, string endpoint)
    {
        var content = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            Console.Error.WriteLine($"[API Error] {response.StatusCode} for {endpoint}: {content}");
            
            if ((int)response.StatusCode == 401)
            {
                await _authService.RefreshTokenAsync();
            }
            
            return null;
        }
        
        if (string.IsNullOrWhiteSpace(content))
            return null;
        
        try
        {
            var json = JsonNode.Parse(content);
            return json as JsonObject;
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"[API] JSON parse error for {endpoint}: {ex.Message}");
            return null;
        }
    }
}
