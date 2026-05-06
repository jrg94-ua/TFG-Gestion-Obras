using System.Net.Http.Json;
using GestionObras.Core.Contracts.Proyectos;
using GestionObras.Core.Contracts.RRHH;

namespace GestionObras.Web.Services;

public sealed class ProyectosApiClient
{
    private readonly HttpClient _httpClient;

    public ProyectosApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProyectosResponse> ObtenerProyectosAsync(string usuarioId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<ProyectosResponse>($"/api/proyectos/{Uri.EscapeDataString(usuarioId)}", cancellationToken)
               ?? new ProyectosResponse();
    }

    public async Task<OperacionResponse> GuardarProyectoAsync(GuardarProyectoRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/api/proyectos", request, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<OperacionResponse> EliminarProyectoAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"/api/proyectos/{id}", cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }
}
