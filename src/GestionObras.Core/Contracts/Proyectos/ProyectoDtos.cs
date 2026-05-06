using GestionObras.Core.Entities;
using GestionObras.Core.Contracts.JefeObra;

namespace GestionObras.Core.Contracts.Proyectos;

public sealed record ProyectoResumenGestionDto(
    int Id,
    string Nombre,
    string Provincia,
    string Municipio,
    TipoSuelo TipoSuelo,
    ZonaClimatica ZonaClimatica,
    DateTime FechaInicio,
    DateTime? FechaFin,
    EstadoProyecto Estado,
    string? ResponsableId,
    string? ResponsableNombreCompleto,
    int NumeroTareas,
    bool UsuarioAsignado);

public sealed class ProyectosResponse
{
    public List<ProyectoResumenGestionDto> Proyectos { get; init; } = new();
    public List<UsuarioResumenDto> ResponsablesDisponibles { get; init; } = new();
    public bool EsAdministrador { get; init; }
    public List<string> RolesUsuarioActual { get; init; } = new();
}

public sealed class GuardarProyectoRequest
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Provincia { get; init; } = string.Empty;
    public string Municipio { get; init; } = string.Empty;
    public TipoSuelo TipoSuelo { get; init; }
    public ZonaClimatica ZonaClimatica { get; init; }
    public DateTime FechaInicio { get; init; }
    public DateTime? FechaFin { get; init; }
    public EstadoProyecto Estado { get; init; }
    public string? ResponsableId { get; init; }
}
