using GestionObras.Core.Entities;

namespace GestionObras.Core.Contracts.JefeObra;

public sealed record ProyectoResumenDto(int Id, string Nombre);

public sealed record UsuarioResumenDto(string Id, string NombreCompleto, string? Cargo);

public sealed record HorarioAsignadoDto(
    int Id,
    string UsuarioId,
    string UsuarioNombreCompleto,
    string? UsuarioCargo,
    int? ProyectoId,
    string? ProyectoNombre,
    DiaSemana DiaSemana,
    TimeOnly HoraEntrada,
    TimeOnly HoraSalida,
    TipoTurno TipoTurno,
    double HorasPrevistas);

public sealed record RegistroFichajeDto(
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

public sealed class JefeObraHorariosResponse
{
    public List<ProyectoResumenDto> Proyectos { get; init; } = new();
    public List<UsuarioResumenDto> Operarios { get; init; } = new();
    public List<HorarioAsignadoDto> Horarios { get; init; } = new();
}

public sealed class JefeObraFichajesResponse
{
    public List<ProyectoResumenDto> Proyectos { get; init; } = new();
    public List<RegistroFichajeDto> Fichajes { get; init; } = new();
}
