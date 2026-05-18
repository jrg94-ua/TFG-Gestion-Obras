using System.Net.Http.Json;
using GestionObras.Core.Contracts.Administracion;
using GestionObras.Core.Contracts.RRHH;
using GestionObras.Core.Entities;

namespace GestionObras.Web.Services;

public sealed class AdministracionApiClient
{
    private readonly HttpClient _httpClient;

    public AdministracionApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GestionUsuariosResponse> ObtenerUsuariosAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<GestionUsuariosResponse>("/api/administracion/usuarios", cancellationToken)
               ?? new GestionUsuariosResponse();
    }

    public async Task<OperacionResponse> GuardarUsuarioAsync(GuardarUsuarioAdminRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/api/administracion/usuarios", request, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<OperacionResponse> EliminarUsuarioAsync(string id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"/api/administracion/usuarios/{Uri.EscapeDataString(id)}", cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<GestionEmpleadosResponse> ObtenerEmpleadosAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<GestionEmpleadosResponse>("/api/administracion/empleados", cancellationToken)
               ?? new GestionEmpleadosResponse();
    }

    public async Task<OperacionResponse> GuardarEmpleadoAsync(GuardarEmpleadoRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/api/administracion/empleados", request, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<OperacionResponse> DesactivarEmpleadoAsync(string usuarioId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"/api/administracion/empleados/{Uri.EscapeDataString(usuarioId)}", cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<TableroProyectosResponse> ObtenerTableroProyectosAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<TableroProyectosResponse>("/api/administracion/tablero-proyectos", cancellationToken)
               ?? new TableroProyectosResponse();
    }

    public async Task<OperacionResponse> CambiarEstadoProyectoAsync(int id, EstadoProyecto estado, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"/api/administracion/tablero-proyectos/{id}/estado/{estado}", null, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<MiTableroResponse> ObtenerMiTableroAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<MiTableroResponse>("/api/administracion/mi-tablero", cancellationToken)
               ?? new MiTableroResponse();
    }

    public async Task<OperacionResponse> CambiarEstadoTareaAsync(int id, EstadoTarea estado, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"/api/administracion/mi-tablero/tareas/{id}/estado", new CambiarEstadoTareaPersonalRequest { Estado = estado }, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<OperacionResponse> BloquearTareaAsync(int id, BloquearTareaPersonalRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"/api/administracion/mi-tablero/tareas/{id}/bloquear", request, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<OperacionResponse> DesbloquearTareaAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"/api/administracion/mi-tablero/tareas/{id}/desbloquear", null, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<OperacionResponse> FinalizarTareaOperarioAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"/api/administracion/mi-tablero/tareas/{id}/finalizar", null, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }
}
