using GestionObras.Core.Entities;
using GestionObras.Core.Contracts.JefeObra;

namespace GestionObras.Core.Contracts.RRHH;

public sealed record ContratoResumenDto(
    int Id,
    string UsuarioId,
    string UsuarioNombreCompleto,
    TipoContrato TipoContrato,
    JornadaLaboral Jornada,
    double HorasSemanales,
    decimal SalarioBrutoAnual,
    DateOnly FechaInicio,
    DateOnly? FechaFin,
    string? CentroTrabajo,
    string? CategoriaConvenio,
    string? NumeroSeguridadSocial,
    string? Observaciones,
    bool Activo,
    DateTime FechaCreacion,
    DateTime? FechaModificacion);

public sealed record RegistroFichajeGestionDto(
    int Id,
    string UsuarioId,
    string UsuarioNombreCompleto,
    DateOnly Fecha,
    DateTime HoraEntrada,
    DateTime? HoraSalida,
    int? ProyectoId,
    string? ProyectoNombre,
    EstadoFichaje Estado,
    double? TotalHoras);

public sealed class RRHHDashboardResponse
{
    public int TotalTrabajadores { get; init; }
    public int ContratosActivos { get; init; }
    public int FichajesPendientes { get; init; }
    public int EnJornadaAhora { get; init; }
    public List<RegistroFichajeGestionDto> FichajesHoy { get; init; } = new();
    public List<ContratoResumenDto> ContratosRecientes { get; init; } = new();
    public List<ContratoResumenDto> ContratosProximosVencer { get; init; } = new();
}

public sealed class RRHHFichajesResponse
{
    public List<ProyectoResumenDto> Proyectos { get; init; } = new();
    public List<UsuarioResumenDto> Trabajadores { get; init; } = new();
    public List<RegistroFichajeGestionDto> Fichajes { get; init; } = new();
}

public sealed class RRHHContratosResponse
{
    public List<UsuarioResumenDto> Trabajadores { get; init; } = new();
    public List<ContratoResumenDto> Contratos { get; init; } = new();
    public int ContratosProximosVencer { get; init; }
}

public sealed class GuardarContratoRequest
{
    public int Id { get; init; }
    public string UsuarioId { get; init; } = string.Empty;
    public TipoContrato TipoContrato { get; init; }
    public DateOnly FechaInicio { get; init; }
    public DateOnly? FechaFin { get; init; }
    public decimal SalarioBrutoAnual { get; init; }
    public JornadaLaboral Jornada { get; init; }
    public double HorasSemanales { get; init; }
    public string? CentroTrabajo { get; init; }
    public string? CategoriaConvenio { get; init; }
    public string? NumeroSeguridadSocial { get; init; }
    public string? Observaciones { get; init; }
}

public sealed class OperacionResponse
{
    public bool Correcto { get; init; }
    public string Mensaje { get; init; } = string.Empty;
}
