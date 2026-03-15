using System.Net.Http.Json;
using F1_MlFlow.Models.Common;
using Microsoft.Extensions.Options;

namespace F1_MlFlow.Services.Api;

public abstract class ApiServiceBase(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiOptions)
{
    protected HttpClient Client => httpClientFactory.CreateClient("ApiClient");
    protected ApiSettings Settings => apiOptions.Value;

    protected async Task<ApiResult<T>> GetAsync<T>(string uri, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await Client.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var details = await ReadErrorDetailsAsync(response, cancellationToken);
                return ApiResult<T>.Failure($"Erro HTTP {(int)response.StatusCode} ({response.ReasonPhrase}) ao consultar {BuildEndpoint(uri)}.{details}");
            }

            var data = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
            return data is null
                ? ApiResult<T>.Failure("Resposta vazia da API.")
                : ApiResult<T>.Success(data);
        }
        catch (Exception ex)
        {
            return ApiResult<T>.Failure($"Falha ao consultar API {BuildEndpoint(uri)}: {ex.Message}");
        }
    }

    protected async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string uri, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await Client.PostAsJsonAsync(uri, request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var details = await ReadErrorDetailsAsync(response, cancellationToken);
                return ApiResult<TResponse>.Failure($"Erro HTTP {(int)response.StatusCode} ({response.ReasonPhrase}) ao enviar para {BuildEndpoint(uri)}.{details}");
            }

            var data = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
            return data is null
                ? ApiResult<TResponse>.Failure("Resposta vazia da API.")
                : ApiResult<TResponse>.Success(data);
        }
        catch (Exception ex)
        {
            return ApiResult<TResponse>.Failure($"Falha ao consultar API {BuildEndpoint(uri)}: {ex.Message}");
        }
    }

    private static async Task<string> ReadErrorDetailsAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }

            var trimmed = content.Trim();
            if (trimmed.Length > 200)
            {
                trimmed = $"{trimmed[..200]}...";
            }

            return $" Detalhe: {trimmed}";
        }
        catch
        {
            return string.Empty;
        }
    }

    protected ApiResult<T> NotImplemented<T>(string feature, string requiredInfo)
    {
        return ApiResult<T>.Failure($"Endpoint não implementado no backend para {feature}. Preciso de: {requiredInfo}.");
    }

    private string BuildEndpoint(string uri)
    {
        if (Uri.TryCreate(uri, UriKind.Absolute, out var absolute))
        {
            return absolute.ToString();
        }

        if (Client.BaseAddress is null)
        {
            return uri;
        }

        return new Uri(Client.BaseAddress, uri).ToString();
    }
}
