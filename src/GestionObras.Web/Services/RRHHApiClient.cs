using System.Net.Http.Json;
using GestionObras.Core.Contracts.RRHH;
using GestionObras.Core.Entities;

namespace GestionObras.Web.Services;

public sealed class RRHHApiClient
{
    private readonly HttpClient _httpClient;

    public RRHHApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RRHHDashboardResponse> ObtenerDashboardAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<RRHHDashboardResponse>("/api/rrhh/dashboard", cancellationToken)
               ?? new RRHHDashboardResponse();
    }

    public async Task<RRHHFichajesResponse> ObtenerFichajesAsync(
        DateOnly desde,
        DateOnly hasta,
        int? proyectoId,
        string? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>
        {
            $"desde={desde:yyyy-MM-dd}",
            $"hasta={hasta:yyyy-MM-dd}"
        };

        if (proyectoId.HasValue)
        {
            query.Add($"proyectoId={proyectoId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(usuarioId))
        {
            query.Add($"usuarioId={Uri.EscapeDataString(usuarioId)}");
        }

        return await _httpClient.GetFromJsonAsync<RRHHFichajesResponse>($"/api/rrhh/fichajes?{string.Join("&", query)}", cancellationToken)
               ?? new RRHHFichajesResponse();
    }

    public async Task<OperacionResponse> CambiarEstadoFichajeAsync(int id, EstadoFichaje estado, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"/api/rrhh/fichajes/{id}/estado/{estado}", null, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<OperacionResponse> ValidarPendientesAsync(
        DateOnly desde,
        DateOnly hasta,
        int? proyectoId,
        string? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>
        {
            $"desde={desde:yyyy-MM-dd}",
            $"hasta={hasta:yyyy-MM-dd}"
        };

        if (proyectoId.HasValue)
        {
            query.Add($"proyectoId={proyectoId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(usuarioId))
        {
            query.Add($"usuarioId={Uri.EscapeDataString(usuarioId)}");
        }

        using var response = await _httpClient.PostAsync($"/api/rrhh/fichajes/validar-pendientes?{string.Join("&", query)}", null, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<RRHHContratosResponse> ObtenerContratosAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<RRHHContratosResponse>("/api/rrhh/contratos", cancellationToken)
               ?? new RRHHContratosResponse();
    }

    public async Task<OperacionResponse> GuardarContratoAsync(GuardarContratoRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/api/rrhh/contratos", request, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<OperacionResponse> FinalizarContratoAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"/api/rrhh/contratos/{id}/finalizar", null, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }
}
