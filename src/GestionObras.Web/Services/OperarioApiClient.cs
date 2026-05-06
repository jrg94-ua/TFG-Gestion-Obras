using System.Net.Http.Json;
using GestionObras.Core.Contracts.Operario;

namespace GestionObras.Web.Services;

public sealed class OperarioApiClient
{
    private readonly HttpClient _httpClient;

    public OperarioApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OperarioDashboardResponse> ObtenerDashboardAsync(string usuarioId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<OperarioDashboardResponse>(
                   $"/api/operario/{Uri.EscapeDataString(usuarioId)}/dashboard",
                   cancellationToken)
               ?? new OperarioDashboardResponse();
    }

    public async Task<OperarioFichajeResponse> ObtenerFichajeAsync(string usuarioId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<OperarioFichajeResponse>(
                   $"/api/operario/{Uri.EscapeDataString(usuarioId)}/fichaje",
                   cancellationToken)
               ?? new OperarioFichajeResponse();
    }

    public async Task<OperacionFichajeResponse> FicharEntradaAsync(string usuarioId, int? proyectoId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            $"/api/operario/{Uri.EscapeDataString(usuarioId)}/fichaje/entrada",
            new CrearFichajeRequest { ProyectoId = proyectoId },
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<OperacionFichajeResponse>(cancellationToken: cancellationToken)
               ?? new OperacionFichajeResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<OperacionFichajeResponse> FicharSalidaAsync(string usuarioId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync(
            $"/api/operario/{Uri.EscapeDataString(usuarioId)}/fichaje/salida",
            content: null,
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<OperacionFichajeResponse>(cancellationToken: cancellationToken)
               ?? new OperacionFichajeResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }
}
