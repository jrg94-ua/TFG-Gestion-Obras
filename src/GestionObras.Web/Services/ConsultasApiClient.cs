using System.Net.Http.Json;
using GestionObras.Core.Contracts.Consultas;

namespace GestionObras.Web.Services;

public sealed class ConsultasApiClient
{
    private readonly HttpClient _httpClient;

    public ConsultasApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DashboardGeneralResponse> ObtenerDashboardGeneralAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<DashboardGeneralResponse>("/api/consultas/dashboard", cancellationToken)
               ?? new DashboardGeneralResponse();
    }

    public async Task<AdminDashboardResponse> ObtenerDashboardAdminAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<AdminDashboardResponse>("/api/consultas/admin/dashboard", cancellationToken)
               ?? new AdminDashboardResponse();
    }

    public async Task<JefeObraDashboardResponse> ObtenerDashboardJefeObraAsync(string usuarioId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<JefeObraDashboardResponse>($"/api/consultas/jefe-obra/{Uri.EscapeDataString(usuarioId)}/dashboard", cancellationToken)
               ?? new JefeObraDashboardResponse();
    }

    public async Task<OficinaTecnicaDashboardResponse> ObtenerDashboardOficinaTecnicaAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<OficinaTecnicaDashboardResponse>("/api/consultas/oficina-tecnica/dashboard", cancellationToken)
               ?? new OficinaTecnicaDashboardResponse();
    }

    public async Task<GanttProyectoResponse> ObtenerGanttProyectoAsync(int proyectoId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<GanttProyectoResponse>($"/api/consultas/proyectos/{proyectoId}/gantt", cancellationToken)
               ?? new GanttProyectoResponse();
    }

    public async Task<HistorialMaterialesProyectoResponse> ObtenerHistorialMaterialesProyectoAsync(int proyectoId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<HistorialMaterialesProyectoResponse>($"/api/consultas/proyectos/{proyectoId}/historial-materiales", cancellationToken)
               ?? new HistorialMaterialesProyectoResponse();
    }
}
