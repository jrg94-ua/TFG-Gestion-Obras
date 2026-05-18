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

    public async Task<OperarioDashboardResponse> ObtenerDashboardAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<OperarioDashboardResponse>(
                   "/api/operario/dashboard",
                   cancellationToken)
               ?? new OperarioDashboardResponse();
    }

    public async Task<OperarioFichajeResponse> ObtenerFichajeAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<OperarioFichajeResponse>(
                   "/api/operario/fichaje",
                   cancellationToken)
               ?? new OperarioFichajeResponse();
    }

    public async Task<OperacionFichajeResponse> FicharEntradaAsync(int? proyectoId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/operario/fichaje/entrada",
            new CrearFichajeRequest { ProyectoId = proyectoId },
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<OperacionFichajeResponse>(cancellationToken: cancellationToken)
               ?? new OperacionFichajeResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<OperacionFichajeResponse> FicharSalidaAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync(
            "/api/operario/fichaje/salida",
            content: null,
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<OperacionFichajeResponse>(cancellationToken: cancellationToken)
               ?? new OperacionFichajeResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }
}
