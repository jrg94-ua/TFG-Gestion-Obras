using GestionObras.Core.Entities;
using GestionObras.Core.Contracts.JefeObra;

namespace GestionObras.Core.Contracts.Operario;

public sealed record TareaResumenDto(
    int Id,
    string Nombre,
    string? ProyectoNombre,
    EstadoTarea Estado,
    DateTime FechaInicio);

public sealed record HorarioResumenDto(
    int Id,
    DiaSemana DiaSemana,
    TimeOnly HoraEntrada,
    TimeOnly HoraSalida,
    TipoTurno TipoTurno,
    double HorasPrevistas,
    string? ProyectoNombre);

public sealed record RegistroFichajeResumenDto(
    int Id,
    DateOnly Fecha,
    DateTime HoraEntrada,
    DateTime? HoraSalida,
    string? ProyectoNombre,
    EstadoFichaje Estado,
    double? TotalHoras);

public sealed class OperarioDashboardResponse
{
    public string NombreUsuario { get; init; } = string.Empty;
    public int TareasPendientes { get; init; }
    public int TareasEnCurso { get; init; }
    public int TareasCompletadas { get; init; }
    public double HorasSemanales { get; init; }
    public List<TareaResumenDto> TareasActivas { get; init; } = new();
    public List<HorarioResumenDto> HorariosSemanales { get; init; } = new();
    public RegistroFichajeResumenDto? UltimoFichaje { get; init; }
}

public sealed class OperarioFichajeResponse
{
    public string NombreUsuario { get; init; } = string.Empty;
    public RegistroFichajeResumenDto? FichajeAbierto { get; init; }
    public List<RegistroFichajeResumenDto> Historial { get; init; } = new();
    public List<HorarioResumenDto> HorarioHoy { get; init; } = new();
    public List<ProyectoResumenDto> ProyectosDisponibles { get; init; } = new();
}

public sealed class CrearFichajeRequest
{
    public int? ProyectoId { get; init; }
}

public sealed class OperacionFichajeResponse
{
    public bool Correcto { get; init; }
    public string Mensaje { get; init; } = string.Empty;
}
