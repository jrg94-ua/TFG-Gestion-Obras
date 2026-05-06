using GestionObras.Core.Entities;
using GestionObras.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestionObras.Web.Services;

public sealed class KanbanService
{
    private readonly GestionObrasDbContext _db;
    private readonly UserManager<UsuarioObra> _userManager;

    public KanbanService(GestionObrasDbContext db, UserManager<UsuarioObra> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<KanbanBoardContext> CargarTableroAsync(int proyectoId, string? usuarioActualId)
    {
        var proyecto = await _db.Proyectos
            .Include(p => p.Responsable)
            .Include(p => p.Tareas)
                .ThenInclude(t => t.SubTareas)
            .Include(p => p.Tareas)
                .ThenInclude(t => t.Bloqueo)
            .Include(p => p.Tareas)
                .ThenInclude(t => t.UsuariosAsignados)
            .Include(p => p.Tareas)
                .ThenInclude(t => t.ResponsableFinal)
            .Include(p => p.Tareas)
                .ThenInclude(t => t.Predecesoras)
            .Include(p => p.Tareas)
                .ThenInclude(t => t.Firmas)
            .Include(p => p.Tareas)
                .ThenInclude(t => t.Documentos)
            .Include(p => p.Tareas)
                .ThenInclude(t => t.CompletadaPor)
            .FirstOrDefaultAsync(p => p.Id == proyectoId);

        var rolesUsuarioActual = new HashSet<string>();
        if (!string.IsNullOrWhiteSpace(usuarioActualId))
        {
            var usuarioActual = await _userManager.FindByIdAsync(usuarioActualId);
            if (usuarioActual != null)
            {
                rolesUsuarioActual = (await _userManager.GetRolesAsync(usuarioActual)).ToHashSet();
            }
        }

        var empleadosDisponibles = await _db.Users
            .Where(u => u.Activo)
            .OrderBy(u => u.NombreCompleto)
            .ToListAsync();

        var trabajadoresFinalesDisponibles = new List<UsuarioObra>();
        foreach (var empleado in empleadosDisponibles)
        {
            var roles = await _userManager.GetRolesAsync(empleado);
            if (UsuarioEsTrabajadorFinal(roles))
            {
                trabajadoresFinalesDisponibles.Add(empleado);
            }
        }

        return new KanbanBoardContext
        {
            Proyecto = proyecto,
            Tareas = proyecto?.Tareas.ToList() ?? new List<Tarea>(),
            EmpleadosDisponibles = empleadosDisponibles,
            TrabajadoresFinalesDisponibles = trabajadoresFinalesDisponibles,
            RolesUsuarioActual = rolesUsuarioActual,
            EsResponsable = proyecto != null &&
                            (proyecto.ResponsableId == usuarioActualId || rolesUsuarioActual.Contains("Administrador"))
        };
    }

    public async Task<List<UsuarioObra>> ObtenerResponsablesDisponiblesAsync(
        IReadOnlyCollection<UsuarioObra> empleadosDisponibles,
        bool esSubtarea,
        IReadOnlyCollection<string> rolesUsuarioActual)
    {
        if (esSubtarea)
        {
            var listaSubtarea = new List<UsuarioObra>();
            foreach (var empleado in empleadosDisponibles)
            {
                var rolesEmpleado = await _userManager.GetRolesAsync(empleado);
                if (UsuarioEsAsignableEnSubtarea(rolesUsuarioActual, rolesEmpleado))
                {
                    listaSubtarea.Add(empleado);
                }
            }

            return listaSubtarea;
        }

        var responsablesPrincipales = new List<UsuarioObra>();
        foreach (var empleado in empleadosDisponibles)
        {
            var rolesEmpleado = await _userManager.GetRolesAsync(empleado);
            if (rolesEmpleado.Contains("Administrador") ||
                rolesEmpleado.Contains("JefeObra") ||
                rolesEmpleado.Contains("OficinaTecnica"))
            {
                responsablesPrincipales.Add(empleado);
            }
        }

        return responsablesPrincipales;
    }

    public async Task ActualizarEstadoTareaAsync(int tareaId, EstadoTarea nuevoEstado)
    {
        var tarea = await _db.Tareas
            .Include(t => t.Bloqueo)
            .FirstOrDefaultAsync(t => t.Id == tareaId);

        if (tarea == null)
        {
            throw new InvalidOperationException("La tarea indicada no existe.");
        }

        if (tarea.Estado == EstadoTarea.Bloqueado && nuevoEstado != EstadoTarea.Bloqueado && tarea.Bloqueo != null)
        {
            tarea.Bloqueo.FechaResolucion = DateTime.Now;
        }

        tarea.Estado = nuevoEstado;
        await _db.SaveChangesAsync();
    }

    public async Task GuardarTareaAsync(Tarea tareaEditando, IReadOnlyCollection<string> responsablesSeleccionados, IReadOnlyCollection<int> dependenciasSeleccionadas)
    {
        var idsDependencias = dependenciasSeleccionadas
            .Where(id => id != tareaEditando.Id)
            .Distinct()
            .ToList();

        var dependencias = await _db.Tareas
            .Where(t => idsDependencias.Contains(t.Id))
            .ToListAsync();

        var responsables = responsablesSeleccionados.Any()
            ? await _db.Users.Where(u => responsablesSeleccionados.Contains(u.Id)).ToListAsync()
            : new List<UsuarioObra>();

        tareaEditando.UsuariosAsignados = responsables;

        if (tareaEditando.Id > 0)
        {
            var existente = await _db.Tareas
                .Include(t => t.UsuariosAsignados)
                .Include(t => t.Predecesoras)
                .FirstOrDefaultAsync(t => t.Id == tareaEditando.Id);

            if (existente == null)
            {
                throw new InvalidOperationException("La tarea a editar ya no existe.");
            }

            existente.Nombre = tareaEditando.Nombre;
            existente.Descripcion = tareaEditando.Descripcion;
            existente.Estado = tareaEditando.Estado;
            existente.FechaInicio = tareaEditando.FechaInicio;
            existente.FechaFin = tareaEditando.FechaFin;
            existente.PresupuestoEstimado = tareaEditando.PresupuestoEstimado;
            existente.CostesReales = tareaEditando.CostesReales;
            existente.HorasSemanalesEstimadas = tareaEditando.HorasSemanalesEstimadas;
            existente.ResponsableFinalId = tareaEditando.ResponsableFinalId;
            existente.Prioridad = tareaEditando.Prioridad;
            existente.RequiereFirmaConjunta = tareaEditando.RequiereFirmaConjunta;

            existente.UsuariosAsignados.Clear();
            foreach (var responsable in responsables)
            {
                existente.UsuariosAsignados.Add(responsable);
            }

            existente.Predecesoras.Clear();
            foreach (var dependencia in dependencias)
            {
                existente.Predecesoras.Add(dependencia);
            }
        }
        else
        {
            tareaEditando.Predecesoras = dependencias;
            _db.Tareas.Add(tareaEditando);
        }

        await _db.SaveChangesAsync();
    }

    public async Task EliminarTareaAsync(int tareaId)
    {
        var tarea = await _db.Tareas.FindAsync(tareaId);
        if (tarea == null)
        {
            return;
        }

        _db.Tareas.Remove(tarea);
        await _db.SaveChangesAsync();
    }

    public async Task BloquearTareaAsync(int tareaId, TipoBloqueo tipoBloqueo, string justificacionBloqueo)
    {
        var tarea = await _db.Tareas
            .Include(t => t.Bloqueo)
            .FirstOrDefaultAsync(t => t.Id == tareaId);

        if (tarea == null)
        {
            throw new InvalidOperationException("La tarea indicada no existe.");
        }

        tarea.Estado = EstadoTarea.Bloqueado;

        if (tarea.Bloqueo == null)
        {
            tarea.Bloqueo = new BloqueoTarea
            {
                TareaId = tarea.Id,
                Tipo = tipoBloqueo,
                JustificacionTecnica = justificacionBloqueo,
                FechaBloqueo = DateTime.Now
            };
        }
        else
        {
            tarea.Bloqueo.Tipo = tipoBloqueo;
            tarea.Bloqueo.JustificacionTecnica = justificacionBloqueo;
            tarea.Bloqueo.FechaBloqueo = DateTime.Now;
            tarea.Bloqueo.FechaResolucion = null;
        }

        var subtareas = await ObtenerSubtareasDescendientesAsync(tarea.Id);
        foreach (var subtarea in subtareas)
        {
            subtarea.Estado = EstadoTarea.Bloqueado;

            if (subtarea.Bloqueo == null)
            {
                subtarea.Bloqueo = new BloqueoTarea
                {
                    TareaId = subtarea.Id,
                    Tipo = tipoBloqueo,
                    JustificacionTecnica = $"Bloqueo heredado de tarea padre '{tarea.Nombre}'. Motivo: {justificacionBloqueo}",
                    FechaBloqueo = DateTime.Now
                };
            }
            else
            {
                subtarea.Bloqueo.Tipo = tipoBloqueo;
                subtarea.Bloqueo.JustificacionTecnica = $"Bloqueo heredado de tarea padre '{tarea.Nombre}'. Motivo: {justificacionBloqueo}";
                subtarea.Bloqueo.FechaBloqueo = DateTime.Now;
                subtarea.Bloqueo.FechaResolucion = null;
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task DesbloquearTareaAsync(int tareaId)
    {
        var tarea = await _db.Tareas
            .Include(t => t.Bloqueo)
            .FirstOrDefaultAsync(t => t.Id == tareaId);

        if (tarea == null || tarea.Bloqueo == null)
        {
            return;
        }

        tarea.Bloqueo.FechaResolucion = DateTime.Now;
        tarea.Estado = EstadoTarea.Pendiente;
        await _db.SaveChangesAsync();
    }

    public async Task CompletarTareaAsync(int tareaId, string usuarioActualId, string? observacionesFinalizacion)
    {
        var tarea = await _db.Tareas
            .Include(t => t.UsuariosAsignados)
            .Include(t => t.Firmas)
            .FirstOrDefaultAsync(t => t.Id == tareaId);

        if (tarea == null)
        {
            throw new InvalidOperationException("La tarea indicada no existe.");
        }

        if (tarea.RequiereFirmaConjunta && !tarea.TodosHanFirmado())
        {
            throw new InvalidOperationException("La tarea requiere la firma de todos los usuarios asignados antes de finalizar.");
        }

        tarea.Estado = EstadoTarea.Finalizado;
        tarea.FechaFinalizacion = DateTime.Now;
        tarea.CompletadaPorId = usuarioActualId;
        tarea.ObservacionesFinalizacion = observacionesFinalizacion;

        await _db.SaveChangesAsync();
    }

    public async Task<FirmaResultado> FirmarTareaAsync(int tareaId, string usuarioActualId, string? observacionesFirma, bool aprobada)
    {
        var tarea = await _db.Tareas
            .Include(t => t.UsuariosAsignados)
            .Include(t => t.Bloqueo)
            .FirstOrDefaultAsync(t => t.Id == tareaId);

        if (tarea == null)
        {
            throw new InvalidOperationException("La tarea indicada no existe.");
        }

        var firmaExistente = await _db.FirmasTareas
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.TareaId == tarea.Id && f.UsuarioId == usuarioActualId);

        if (firmaExistente != null)
        {
            throw new InvalidOperationException("Ya existe una firma registrada para esta tarea y usuario.");
        }

        _db.FirmasTareas.Add(new FirmaTarea
        {
            TareaId = tarea.Id,
            UsuarioId = usuarioActualId,
            FechaFirma = DateTime.Now,
            Observaciones = observacionesFirma,
            Aprobada = aprobada
        });

        await _db.SaveChangesAsync();

        if (aprobada)
        {
            return new FirmaResultado(false);
        }

        tarea.Estado = EstadoTarea.Bloqueado;
        _db.BloqueosTareas.Add(new BloqueoTarea
        {
            TareaId = tarea.Id,
            Tipo = TipoBloqueo.Otro,
            JustificacionTecnica = $"Rechazada por usuario {usuarioActualId}",
            FechaBloqueo = DateTime.Now
        });
        await _db.SaveChangesAsync();

        return new FirmaResultado(true);
    }

    private async Task<List<Tarea>> ObtenerSubtareasDescendientesAsync(int tareaId)
    {
        var proyectoId = await _db.Tareas
            .Where(t => t.Id == tareaId)
            .Select(t => t.ProyectoId)
            .FirstAsync();

        var todasLasTareas = await _db.Tareas
            .Include(t => t.Bloqueo)
            .Where(t => t.ProyectoId == proyectoId)
            .ToListAsync();

        var resultado = new List<Tarea>();
        var pendientes = new Queue<int>();
        pendientes.Enqueue(tareaId);

        while (pendientes.Count > 0)
        {
            var actual = pendientes.Dequeue();
            var hijas = todasLasTareas.Where(t => t.TareaPadreId == actual).ToList();

            foreach (var hija in hijas)
            {
                resultado.Add(hija);
                pendientes.Enqueue(hija.Id);
            }
        }

        return resultado;
    }

    private static bool UsuarioEsTrabajadorFinal(IList<string> rolesEmpleado)
    {
        return rolesEmpleado.Contains("OperarioObra") ||
               rolesEmpleado.Contains("OperarioOficinaT") ||
               rolesEmpleado.Contains("Operario");
    }

    private static bool UsuarioEsAsignableEnSubtarea(
        IReadOnlyCollection<string> rolesUsuarioActual,
        IList<string> rolesEmpleado)
    {
        if (rolesUsuarioActual.Contains("Administrador"))
        {
            return true;
        }

        if (rolesUsuarioActual.Contains("JefeObra"))
        {
            return rolesEmpleado.Contains("OperarioObra") ||
                   rolesEmpleado.Contains("Operario") ||
                   rolesEmpleado.Contains("JefeObra") ||
                   rolesEmpleado.Contains("OficinaTecnica") ||
                   rolesEmpleado.Contains("Administrador");
        }

        if (rolesUsuarioActual.Contains("OficinaTecnica"))
        {
            return rolesEmpleado.Contains("OperarioOficinaT") ||
                   rolesEmpleado.Contains("Operario") ||
                   rolesEmpleado.Contains("OficinaTecnica") ||
                   rolesEmpleado.Contains("JefeObra") ||
                   rolesEmpleado.Contains("Administrador");
        }

        return true;
    }
}

public sealed class KanbanBoardContext
{
    public Proyecto? Proyecto { get; init; }
    public List<Tarea> Tareas { get; init; } = new();
    public List<UsuarioObra> EmpleadosDisponibles { get; init; } = new();
    public List<UsuarioObra> TrabajadoresFinalesDisponibles { get; init; } = new();
    public HashSet<string> RolesUsuarioActual { get; init; } = new();
    public bool EsResponsable { get; init; }
}

public sealed record FirmaResultado(bool TareaBloqueadaPorRechazo);
