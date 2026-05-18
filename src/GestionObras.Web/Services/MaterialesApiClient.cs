using System.Net.Http.Json;
using GestionObras.Core.Contracts.Materiales;
using GestionObras.Core.Contracts.RRHH;

namespace GestionObras.Web.Services;

public sealed class MaterialesApiClient
{
    private readonly HttpClient _httpClient;

    public MaterialesApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<MaterialesGestionResponse> ObtenerMaterialesGestionAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<MaterialesGestionResponse>("/api/materiales/gestion", cancellationToken)
               ?? new MaterialesGestionResponse();
    }

    public async Task<OperacionResponse> GuardarMaterialAsync(GuardarMaterialRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/api/materiales", request, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<OperacionResponse> EliminarMaterialAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"/api/materiales/{id}", cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<OperacionResponse> AlternarMaterialAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"/api/materiales/{id}/alternar", null, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<CatalogosMaterialesResponse> ObtenerCatalogosAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<CatalogosMaterialesResponse>("/api/materiales/catalogos", cancellationToken)
               ?? new CatalogosMaterialesResponse();
    }

    public async Task<OperacionResponse> GuardarProveedorAsync(GuardarProveedorRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/api/materiales/proveedores", request, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<OperacionResponse> GuardarCategoriaAsync(GuardarCategoriaMaterialRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/api/materiales/categorias", request, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<OperacionResponse> AlternarProveedorAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"/api/materiales/proveedores/{id}/alternar", null, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<OperacionResponse> AlternarCategoriaAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"/api/materiales/categorias/{id}/alternar", null, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<SolicitarMaterialesResponse> ObtenerDatosSolicitudAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<SolicitarMaterialesResponse>("/api/materiales/solicitudes/jefe-obra", cancellationToken)
               ?? new SolicitarMaterialesResponse();
    }

    public async Task<OperacionResponse> CrearSolicitudAsync(CrearSolicitudMaterialRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("/api/materiales/solicitudes", request, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<MisSolicitudesMaterialesResponse> ObtenerMisSolicitudesAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<MisSolicitudesMaterialesResponse>("/api/materiales/solicitudes/jefe-obra/historial", cancellationToken)
               ?? new MisSolicitudesMaterialesResponse();
    }

    public async Task<OperacionResponse> CancelarSolicitudAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"/api/materiales/solicitudes/{id}/cancelar", null, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }

    public async Task<GestionSolicitudesMaterialesResponse> ObtenerSolicitudesAdminAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<GestionSolicitudesMaterialesResponse>("/api/materiales/solicitudes/admin", cancellationToken)
               ?? new GestionSolicitudesMaterialesResponse();
    }

    public async Task<OperacionResponse> RevisarSolicitudAsync(int id, RevisarSolicitudMaterialRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"/api/materiales/solicitudes/{id}/revisar", request, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
               ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
    }
}
