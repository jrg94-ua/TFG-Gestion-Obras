using GestionObras.Core.Contracts.Materiales;
using GestionObras.Core.Entities;

namespace GestionObras.Core.Contracts.Consultas;

public sealed record ProyectoConsultaDto(
    int Id,
    string Nombre,
    string? Municipio,
    string? Provincia,
    EstadoProyecto Estado,
    DateTime FechaInicio,
    int NumeroTareas,
    bool TienePresupuesto);

public sealed class DashboardGeneralResponse
{
    public int TotalProyectos { get; init; }
    public int ProyectosEnCurso { get; init; }
    public int TotalTareas { get; init; }
    public int TareasBloqueadas { get; init; }
    public int TotalEmpleados { get; init; }
    public int TotalMateriales { get; init; }
    public List<string> Alertas { get; init; } = new();
    public List<ProyectoConsultaDto> ProyectosRecientes { get; init; } = new();
}

public sealed class AdminDashboardResponse
{
    public int TotalProyectos { get; init; }
    public int TotalUsuarios { get; init; }
    public int TotalEmpleados { get; init; }
    public int TotalFacturas { get; init; }
    public List<ProyectoConsultaDto> ProyectosRecientes { get; init; } = new();
}

public sealed class JefeObraDashboardResponse
{
    public int ProyectosActivos { get; init; }
    public int TareasPendientes { get; init; }
    public int TareasBloqueadas { get; init; }
    public int TotalMateriales { get; init; }
    public List<ProyectoConsultaDto> MisProyectos { get; init; } = new();
}

public sealed class OficinaTecnicaDashboardResponse
{
    public int TotalCarpetas { get; init; }
    public int TotalPresupuestos { get; init; }
    public int ProyectosPlanificacion { get; init; }
    public int TotalFacturas { get; init; }
    public List<ProyectoConsultaDto> ProyectosPlanificacionLista { get; init; } = new();
}

public sealed record ProyectoGanttDto(int Id, string Nombre);

public sealed record TareaGanttDto(
    int Id,
    string Nombre,
    EstadoTarea Estado,
    DateTime FechaInicio,
    DateTime? FechaFin,
    int Nivel,
    PrioridadTarea Prioridad);

public sealed class GanttProyectoResponse
{
    public ProyectoGanttDto? Proyecto { get; init; }
    public List<TareaGanttDto> Tareas { get; init; } = new();
}

public sealed class HistorialMaterialesProyectoResponse
{
    public ProyectoGanttDto? Proyecto { get; init; }
    public List<SolicitudMaterialResumenDto> Solicitudes { get; init; } = new();
}
