using System.Net.Http.Json;
using GestionObras.Core.Contracts.JefeObra;

namespace GestionObras.Web.Services;

public sealed class JefeObraApiClient
{
    private readonly HttpClient _httpClient;

    public JefeObraApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<JefeObraHorariosResponse> ObtenerHorariosAsync(
        string responsableId,
        int? proyectoId,
        string? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (proyectoId.HasValue)
        {
            query.Add($"proyectoId={proyectoId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(usuarioId))
        {
            query.Add($"usuarioId={Uri.EscapeDataString(usuarioId)}");
        }

        var url = $"/api/jefe-obra/{Uri.EscapeDataString(responsableId)}/horarios";
        if (query.Any())
        {
            url += "?" + string.Join("&", query);
        }

        return await _httpClient.GetFromJsonAsync<JefeObraHorariosResponse>(url, cancellationToken)
               ?? new JefeObraHorariosResponse();
    }

    public async Task<JefeObraFichajesResponse> ObtenerFichajesAsync(
        string responsableId,
        DateOnly desde,
        DateOnly hasta,
        int? proyectoId,
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

        var url = $"/api/jefe-obra/{Uri.EscapeDataString(responsableId)}/fichajes?{string.Join("&", query)}";

        return await _httpClient.GetFromJsonAsync<JefeObraFichajesResponse>(url, cancellationToken)
               ?? new JefeObraFichajesResponse();
    }
}
