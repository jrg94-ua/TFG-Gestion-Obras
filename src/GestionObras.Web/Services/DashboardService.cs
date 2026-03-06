using GestionObras.Core.Entities;
using GestionObras.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionObras.Web.Services;

public class DashboardStats
{
    public int TotalProyectos { get; set; }
    public int ProyectosEnCurso { get; set; }
    public int TotalTareas { get; set; }
    public int TareasBloqueadas { get; set; }
    public int TotalEmpleados { get; set; }
    public int TotalMateriales { get; set; }
    public List<Proyecto> ProyectosRecientes { get; set; } = new();
    public List<string> Alertas { get; set; } = new();
}

public class DashboardService
{
    private readonly GestionObrasDbContext _db;

    public DashboardService(GestionObrasDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardStats> ObtenerEstadisticasAsync()
    {
        var stats = new DashboardStats
        {
            TotalProyectos = await _db.Proyectos.CountAsync(),
            ProyectosEnCurso = await _db.Proyectos
                .CountAsync(p => p.Estado == EstadoProyecto.EnCurso),
            TotalTareas = await _db.Tareas
                .CountAsync(t => t.Estado != EstadoTarea.Finalizado),
            TareasBloqueadas = await _db.Tareas
                .CountAsync(t => t.Estado == EstadoTarea.Bloqueado),
            TotalEmpleados = await _db.Empleados.CountAsync(),
            TotalMateriales = await _db.Materiales.CountAsync(),
            ProyectosRecientes = await _db.Proyectos
                .OrderByDescending(p => p.FechaInicio)
                .Take(5)
                .ToListAsync()
        };

        GenerarAlertas(stats);

        return stats;
    }

    private static void GenerarAlertas(DashboardStats stats)
    {
        if (stats.TareasBloqueadas > 0)
            stats.Alertas.Add($"{stats.TareasBloqueadas} tareas bloqueadas requieren atención");

        if (stats.TotalProyectos == 0)
            stats.Alertas.Add("No hay proyectos registrados. Comienza creando tu primer proyecto.");

        var proyectosBloqueados = stats.ProyectosRecientes.Count(p => p.Estado == EstadoProyecto.Bloqueado);
        if (proyectosBloqueados > 0)
            stats.Alertas.Add($"{proyectosBloqueados} proyectos están en estado bloqueado");
    }
}
