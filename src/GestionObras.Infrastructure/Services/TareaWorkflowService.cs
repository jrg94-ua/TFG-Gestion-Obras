using GestionObras.Core.Entities;
using GestionObras.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionObras.Infrastructure.Services;

public sealed class TareaWorkflowService
{
    private readonly GestionObrasDbContext _db;

    public TareaWorkflowService(GestionObrasDbContext db)
    {
        _db = db;
    }

    public async Task ActualizarEstadoAsync(int tareaId, EstadoTarea nuevoEstado)
    {
        var tarea = await CargarTareaParaWorkflowAsync(tareaId);
        if (tarea == null)
        {
            throw new InvalidOperationException("La tarea indicada no existe.");
        }

        await ValidarCambioEstadoAsync(tarea, nuevoEstado);

        if (tarea.Estado == EstadoTarea.Bloqueado && nuevoEstado != EstadoTarea.Bloqueado && tarea.Bloqueo != null)
        {
            tarea.Bloqueo.FechaResolucion = DateTime.Now;
        }

        tarea.Estado = nuevoEstado;
        await _db.SaveChangesAsync();
    }

    public async Task BloquearAsync(int tareaId, TipoBloqueo tipoBloqueo, string justificacionBloqueo)
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

    public async Task DesbloquearAsync(int tareaId)
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

    public async Task CompletarAsync(int tareaId, string usuarioActualId, string? observacionesFinalizacion)
    {
        var tarea = await CargarTareaParaWorkflowAsync(tareaId);
        if (tarea == null)
        {
            throw new InvalidOperationException("La tarea indicada no existe.");
        }

        await ValidarCambioEstadoAsync(tarea, EstadoTarea.Finalizado);

        if (tarea.RequiereFirmaConjunta && !UsuarioEstaAsignadoATarea(tarea, usuarioActualId))
        {
            throw new InvalidOperationException("Solo un usuario asignado a la tarea puede completarla cuando requiere firma conjunta.");
        }

        if (tarea.Bloqueo != null && tarea.Bloqueo.FechaResolucion == null)
        {
            tarea.Bloqueo.FechaResolucion = DateTime.Now;
        }

        tarea.Estado = EstadoTarea.Finalizado;
        tarea.FechaFinalizacion = DateTime.Now;
        tarea.CompletadaPorId = usuarioActualId;
        tarea.ObservacionesFinalizacion = observacionesFinalizacion;

        await _db.SaveChangesAsync();
    }

    public async Task<FirmaResultado> FirmarAsync(int tareaId, string usuarioActualId, string? observacionesFirma, bool aprobada)
    {
        var tarea = await _db.Tareas
            .Include(t => t.UsuariosAsignados)
            .Include(t => t.Bloqueo)
            .Include(t => t.Firmas)
            .FirstOrDefaultAsync(t => t.Id == tareaId);

        if (tarea == null)
        {
            throw new InvalidOperationException("La tarea indicada no existe.");
        }

        ValidarFirma(tarea, usuarioActualId);

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

    public async Task ValidarCambioEstadoAsync(Tarea tarea, EstadoTarea nuevoEstado)
    {
        var quiereAvanzar = nuevoEstado == EstadoTarea.EnCurso || nuevoEstado == EstadoTarea.Finalizado;

        if (quiereAvanzar)
        {
            var predecesorasPendientes = tarea.Predecesoras
                .Where(p => p.Estado != EstadoTarea.Finalizado)
                .Select(p => p.Nombre)
                .ToList();

            if (predecesorasPendientes.Any())
            {
                throw new InvalidOperationException("No puedes avanzar esta tarea porque tiene dependencias sin finalizar: " + string.Join(", ", predecesorasPendientes));
            }
        }

        if (nuevoEstado == EstadoTarea.Finalizado)
        {
            var tieneSubtareasAbiertas = await _db.Tareas
                .AnyAsync(t => t.TareaPadreId == tarea.Id && t.Estado != EstadoTarea.Finalizado);

            if (tieneSubtareasAbiertas)
            {
                throw new InvalidOperationException("No puedes finalizar una tarea padre mientras tenga subtareas no finalizadas.");
            }
        }

        var tareaPadre = tarea.TareaPadre;
        if (tareaPadre == null && tarea.TareaPadreId.HasValue)
        {
            tareaPadre = await _db.Tareas.FirstOrDefaultAsync(t => t.Id == tarea.TareaPadreId.Value);
        }

        if (tareaPadre != null)
        {
            if (quiereAvanzar && (tareaPadre.Estado == EstadoTarea.Pendiente || tareaPadre.Estado == EstadoTarea.Bloqueado))
            {
                throw new InvalidOperationException("No puedes avanzar una subtarea si su tarea padre esta pendiente o bloqueada.");
            }

            if (tareaPadre.Estado == EstadoTarea.Finalizado && nuevoEstado != EstadoTarea.Finalizado)
            {
                throw new InvalidOperationException("No puedes reabrir una subtarea si su tarea padre ya esta finalizada.");
            }
        }

        if (nuevoEstado == EstadoTarea.Finalizado && tarea.RequiereFirmaConjunta && !tarea.TodosHanFirmado())
        {
            throw new InvalidOperationException("La tarea requiere la firma de todos los usuarios asignados antes de finalizar.");
        }
    }

    private static void ValidarFirma(Tarea tarea, string usuarioActualId)
    {
        if (!tarea.RequiereFirmaConjunta)
        {
            throw new InvalidOperationException("Esta tarea no requiere firma conjunta.");
        }

        if (tarea.Estado != EstadoTarea.EnCurso)
        {
            throw new InvalidOperationException("Solo se puede firmar una tarea que este en estado En Curso.");
        }

        if (!UsuarioEstaAsignadoATarea(tarea, usuarioActualId))
        {
            throw new InvalidOperationException("Solo los usuarios asignados a la tarea pueden firmarla.");
        }
    }

    private async Task<Tarea?> CargarTareaParaWorkflowAsync(int tareaId)
    {
        return await _db.Tareas
            .Include(t => t.Bloqueo)
            .Include(t => t.Predecesoras)
            .Include(t => t.UsuariosAsignados)
            .Include(t => t.Firmas)
            .Include(t => t.TareaPadre)
            .FirstOrDefaultAsync(t => t.Id == tareaId);
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

    private static bool UsuarioEstaAsignadoATarea(Tarea tarea, string? usuarioId)
    {
        if (string.IsNullOrWhiteSpace(usuarioId))
        {
            return false;
        }

        return tarea.UsuariosAsignados.Any(u => u.Id == usuarioId) || tarea.ResponsableFinalId == usuarioId;
    }
}

public sealed record FirmaResultado(bool TareaBloqueadaPorRechazo);
