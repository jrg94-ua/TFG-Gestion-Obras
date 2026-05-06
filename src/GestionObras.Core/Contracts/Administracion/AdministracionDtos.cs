using GestionObras.Core.Entities;

namespace GestionObras.Core.Contracts.Administracion;

public sealed class UsuarioGestionDto
{
    public string Id { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string NombreCompleto { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string DNI { get; init; } = string.Empty;
    public bool EmailConfirmed { get; init; }
    public bool Activo { get; init; }
    public List<string> Roles { get; init; } = new();
}

public sealed class GestionUsuariosResponse
{
    public List<UsuarioGestionDto> Usuarios { get; init; } = new();
}

public sealed class GuardarUsuarioAdminRequest
{
    public string? Id { get; init; }
    public string NombreCompleto { get; init; } = string.Empty;
    public string DNI { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Rol { get; init; } = string.Empty;
}

public sealed class EmpleadoGestionDto
{
    public int Id { get; init; }
    public string UsuarioId { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Apellidos { get; init; } = string.Empty;
    public string DNI { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Telefono { get; init; } = string.Empty;
    public DateTime FechaContratacion { get; init; }
    public string Departamento { get; init; } = string.Empty;
    public string Cargo { get; init; } = string.Empty;
    public string? Direccion { get; init; }
    public bool Activo { get; init; }
}

public sealed class GestionEmpleadosResponse
{
    public List<EmpleadoGestionDto> Empleados { get; init; } = new();
    public string DescripcionJerarquiaActual { get; init; } = string.Empty;
}

public sealed class GuardarEmpleadoRequest
{
    public string? UsuarioId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Apellidos { get; init; } = string.Empty;
    public string DNI { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Telefono { get; init; } = string.Empty;
    public DateTime FechaContratacion { get; init; }
    public string Departamento { get; init; } = string.Empty;
    public string Cargo { get; init; } = string.Empty;
    public string? Direccion { get; init; }
    public bool Activo { get; init; }
}

public sealed class ProyectoTableroDto
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
    public int TotalTareas { get; init; }
    public int TareasPendientes { get; init; }
    public int TareasEnCurso { get; init; }
    public int TareasFinalizadas { get; init; }
    public int TareasBloqueadas { get; init; }
    public int EmpleadosAsignados { get; init; }
}

public sealed class TableroProyectosResponse
{
    public List<ProyectoTableroDto> Proyectos { get; init; } = new();
}

public sealed class BloqueoResumenDto
{
    public int Id { get; init; }
    public TipoBloqueo Tipo { get; init; }
    public string JustificacionTecnica { get; init; } = string.Empty;
    public DateTime FechaBloqueo { get; init; }
    public DateTime? FechaResolucion { get; init; }
}

public sealed class ProyectoMinimoDto
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
}

public sealed class TareaPersonalDto
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public EstadoTarea Estado { get; init; }
    public DateTime FechaInicio { get; init; }
    public DateTime? FechaFin { get; init; }
    public decimal PresupuestoEstimado { get; init; }
    public decimal CostesReales { get; init; }
    public int ProyectoId { get; init; }
    public ProyectoMinimoDto Proyecto { get; init; } = new();
    public int? TareaPadreId { get; init; }
    public string? TareaPadreNombre { get; init; }
    public int Nivel { get; init; }
    public PrioridadTarea Prioridad { get; init; }
    public int UsuariosAsignadosCount { get; init; }
    public BloqueoResumenDto? Bloqueo { get; init; }
    public string? CompletadaPorId { get; init; }
    public DateTime? FechaFinalizacion { get; init; }
    public string? ObservacionesFinalizacion { get; init; }
}

public sealed class MiTableroResponse
{
    public string UsuarioNombreCompleto { get; init; } = string.Empty;
    public string UsuarioId { get; init; } = string.Empty;
    public bool EsOperario { get; init; }
    public List<TareaPersonalDto> Tareas { get; init; } = new();
    public List<ProyectoMinimoDto> Proyectos { get; init; } = new();
}

public sealed class CambiarEstadoTareaPersonalRequest
{
    public EstadoTarea Estado { get; init; }
}

public sealed class BloquearTareaPersonalRequest
{
    public TipoBloqueo Tipo { get; init; }
    public string JustificacionTecnica { get; init; } = string.Empty;
}
