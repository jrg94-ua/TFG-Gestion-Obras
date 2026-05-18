using GestionObras.Core.Entities;
using GestionObras.Infrastructure.Data;
using GestionObras.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestionObras.Web.Services;

public sealed class KanbanService
{
    private readonly GestionObrasDbContext _db;
    private readonly TareaWorkflowService _tareaWorkflowService;
    private readonly UserManager<UsuarioObra>? _userManager;

    public KanbanService(
        GestionObrasDbContext db,
        TareaWorkflowService? tareaWorkflowService = null,
        UserManager<UsuarioObra>? userManager = null)
    {
        _db = db;
        _tareaWorkflowService = tareaWorkflowService ?? new TareaWorkflowService(db);
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
            var userManager = GetUserManager();
            var usuarioActual = await userManager.FindByIdAsync(usuarioActualId);
            if (usuarioActual != null)
            {
                rolesUsuarioActual = (await userManager.GetRolesAsync(usuarioActual)).ToHashSet();
            }
        }

        var empleadosDisponibles = await _db.Users
            .Where(u => u.Activo)
            .OrderBy(u => u.NombreCompleto)
            .ToListAsync();

        var trabajadoresFinalesDisponibles = new List<UsuarioObra>();
        foreach (var empleado in empleadosDisponibles)
        {
            var roles = await GetUserManager().GetRolesAsync(empleado);
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
                var rolesEmpleado = await GetUserManager().GetRolesAsync(empleado);
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
            var rolesEmpleado = await GetUserManager().GetRolesAsync(empleado);
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
        await _tareaWorkflowService.ActualizarEstadoAsync(tareaId, nuevoEstado);
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
                .Include(t => t.Firmas)
                .Include(t => t.TareaPadre)
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

            await _tareaWorkflowService.ValidarCambioEstadoAsync(existente, existente.Estado);
        }
        else
        {
            tareaEditando.Predecesoras = dependencias;
            if (tareaEditando.TareaPadreId.HasValue)
            {
                tareaEditando.TareaPadre = await _db.Tareas.FirstOrDefaultAsync(t => t.Id == tareaEditando.TareaPadreId.Value);
            }

            await _tareaWorkflowService.ValidarCambioEstadoAsync(tareaEditando, tareaEditando.Estado);
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
        await _tareaWorkflowService.BloquearAsync(tareaId, tipoBloqueo, justificacionBloqueo);
    }

    public async Task DesbloquearTareaAsync(int tareaId)
    {
        await _tareaWorkflowService.DesbloquearAsync(tareaId);
    }

    public async Task CompletarTareaAsync(int tareaId, string usuarioActualId, string? observacionesFinalizacion)
    {
        await _tareaWorkflowService.CompletarAsync(tareaId, usuarioActualId, observacionesFinalizacion);
    }

    public async Task<FirmaResultado> FirmarTareaAsync(int tareaId, string usuarioActualId, string? observacionesFirma, bool aprobada)
    {
        return await _tareaWorkflowService.FirmarAsync(tareaId, usuarioActualId, observacionesFirma, aprobada);
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

    private UserManager<UsuarioObra> GetUserManager()
    {
        return _userManager ?? throw new InvalidOperationException("UserManager no disponible para esta operacion.");
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
