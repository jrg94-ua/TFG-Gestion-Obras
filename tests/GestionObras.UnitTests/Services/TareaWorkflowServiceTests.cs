using FluentAssertions;
using GestionObras.Core.Entities;
using GestionObras.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GestionObras.UnitTests.Services;

public class TareaWorkflowServiceTests
{
    [Fact]
    public async Task ActualizarEstadoAsync_ConPredecesoraPendiente_DebeLanzarExcepcion()
    {
        await using var db = TestDbContextFactory.Create();
        var proyecto = new Proyecto
        {
            Id = 101,
            Nombre = "Obra Workflow",
            Provincia = "Valencia",
            Municipio = "Valencia",
            FechaInicio = DateTime.Today
        };

        var predecesora = new Tarea
        {
            Id = 1001,
            Nombre = "Base",
            Proyecto = proyecto,
            ProyectoId = proyecto.Id,
            Estado = EstadoTarea.Pendiente,
            FechaInicio = DateTime.Today
        };

        var tarea = new Tarea
        {
            Id = 1002,
            Nombre = "Acabado",
            Proyecto = proyecto,
            ProyectoId = proyecto.Id,
            Estado = EstadoTarea.Pendiente,
            FechaInicio = DateTime.Today,
            Predecesoras = [predecesora]
        };

        db.Proyectos.Add(proyecto);
        db.Tareas.AddRange(predecesora, tarea);
        await db.SaveChangesAsync();

        var service = new TareaWorkflowService(db);

        var accion = () => service.ActualizarEstadoAsync(tarea.Id, EstadoTarea.EnCurso);

        await accion.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*dependencias sin finalizar*");
    }

    [Fact]
    public async Task DesbloquearAsync_DebeResolverBloqueoYDejarLaTareaPendiente()
    {
        await using var db = TestDbContextFactory.Create();
        var proyecto = new Proyecto
        {
            Id = 102,
            Nombre = "Obra Bloqueada",
            Provincia = "Madrid",
            Municipio = "Madrid",
            FechaInicio = DateTime.Today
        };

        var tarea = new Tarea
        {
            Id = 1003,
            Nombre = "Revision",
            Proyecto = proyecto,
            ProyectoId = proyecto.Id,
            Estado = EstadoTarea.Bloqueado,
            FechaInicio = DateTime.Today,
            Bloqueo = new BloqueoTarea
            {
                Tipo = TipoBloqueo.FaltaMaterial,
                JustificacionTecnica = "Sin stock",
                FechaBloqueo = DateTime.Today
            }
        };

        db.Proyectos.Add(proyecto);
        db.Tareas.Add(tarea);
        await db.SaveChangesAsync();

        var service = new TareaWorkflowService(db);

        await service.DesbloquearAsync(tarea.Id);

        var tareaActualizada = await db.Tareas.Include(t => t.Bloqueo).FirstAsync(t => t.Id == tarea.Id);
        tareaActualizada.Estado.Should().Be(EstadoTarea.Pendiente);
        tareaActualizada.Bloqueo.Should().NotBeNull();
        tareaActualizada.Bloqueo!.FechaResolucion.Should().NotBeNull();
    }

    [Fact]
    public async Task CompletarAsync_DebeResolverBloqueoYRegistrarCierre()
    {
        await using var db = TestDbContextFactory.Create();
        var proyecto = new Proyecto
        {
            Id = 103,
            Nombre = "Obra Finalizacion",
            Provincia = "Sevilla",
            Municipio = "Sevilla",
            FechaInicio = DateTime.Today
        };

        var usuario = new UsuarioObra
        {
            Id = "u-final",
            UserName = "ufinal",
            NombreCompleto = "Usuario Final",
            DNI = "11111111A"
        };

        var tarea = new Tarea
        {
            Id = 1004,
            Nombre = "Remate",
            Proyecto = proyecto,
            ProyectoId = proyecto.Id,
            Estado = EstadoTarea.EnCurso,
            FechaInicio = DateTime.Today,
            Bloqueo = new BloqueoTarea
            {
                Tipo = TipoBloqueo.Otro,
                JustificacionTecnica = "Pendiente de validar",
                FechaBloqueo = DateTime.Today
            }
        };

        db.Users.Add(usuario);
        db.Proyectos.Add(proyecto);
        db.Tareas.Add(tarea);
        await db.SaveChangesAsync();

        var service = new TareaWorkflowService(db);

        await service.CompletarAsync(tarea.Id, usuario.Id, "Cierre desde flujo comun");

        var tareaActualizada = await db.Tareas.Include(t => t.Bloqueo).FirstAsync(t => t.Id == tarea.Id);
        tareaActualizada.Estado.Should().Be(EstadoTarea.Finalizado);
        tareaActualizada.CompletadaPorId.Should().Be(usuario.Id);
        tareaActualizada.ObservacionesFinalizacion.Should().Be("Cierre desde flujo comun");
        tareaActualizada.FechaFinalizacion.Should().NotBeNull();
        tareaActualizada.Bloqueo!.FechaResolucion.Should().NotBeNull();
    }
}
