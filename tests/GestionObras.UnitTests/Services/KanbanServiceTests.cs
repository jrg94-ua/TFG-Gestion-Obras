using FluentAssertions;
using GestionObras.Core.Entities;
using GestionObras.Web.Services;

namespace GestionObras.UnitTests.Services;

public class KanbanServiceTests
{
    [Fact]
    public async Task ActualizarEstadoTareaAsync_ConPredecesoraPendiente_DebeLanzarExcepcion()
    {
        await using var db = TestDbContextFactory.Create();
        var proyecto = new Proyecto
        {
            Id = 1,
            Nombre = "Obra Test",
            Provincia = "Valencia",
            Municipio = "Valencia",
            FechaInicio = DateTime.Today
        };

        var predecesora = new Tarea
        {
            Id = 10,
            Nombre = "Cimentacion",
            Proyecto = proyecto,
            ProyectoId = proyecto.Id,
            Estado = EstadoTarea.Pendiente,
            FechaInicio = DateTime.Today
        };

        var tarea = new Tarea
        {
            Id = 11,
            Nombre = "Estructura",
            Proyecto = proyecto,
            ProyectoId = proyecto.Id,
            Estado = EstadoTarea.Pendiente,
            FechaInicio = DateTime.Today,
            Predecesoras = [predecesora]
        };

        db.Proyectos.Add(proyecto);
        db.Tareas.AddRange(predecesora, tarea);
        await db.SaveChangesAsync();

        var service = new KanbanService(db);

        var accion = () => service.ActualizarEstadoTareaAsync(tarea.Id, EstadoTarea.EnCurso);

        await accion.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*dependencias sin finalizar*");
    }

    [Fact]
    public async Task ActualizarEstadoTareaAsync_SubtareaConPadrePendiente_DebeLanzarExcepcion()
    {
        await using var db = TestDbContextFactory.Create();
        var proyecto = new Proyecto
        {
            Id = 2,
            Nombre = "Obra Test 2",
            Provincia = "Alicante",
            Municipio = "Alicante",
            FechaInicio = DateTime.Today
        };

        var padre = new Tarea
        {
            Id = 20,
            Nombre = "Capitulo principal",
            Proyecto = proyecto,
            ProyectoId = proyecto.Id,
            Estado = EstadoTarea.Pendiente,
            FechaInicio = DateTime.Today
        };

        var subtarea = new Tarea
        {
            Id = 21,
            Nombre = "Subtarea",
            Proyecto = proyecto,
            ProyectoId = proyecto.Id,
            Estado = EstadoTarea.Pendiente,
            FechaInicio = DateTime.Today,
            TareaPadre = padre,
            TareaPadreId = padre.Id
        };

        db.Proyectos.Add(proyecto);
        db.Tareas.AddRange(padre, subtarea);
        await db.SaveChangesAsync();

        var service = new KanbanService(db);

        var accion = () => service.ActualizarEstadoTareaAsync(subtarea.Id, EstadoTarea.EnCurso);

        await accion.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*tarea padre esta pendiente o bloqueada*");
    }

    [Fact]
    public async Task CompletarTareaAsync_TareaPadreConSubtareasAbiertas_DebeLanzarExcepcion()
    {
        await using var db = TestDbContextFactory.Create();
        var proyecto = new Proyecto
        {
            Id = 3,
            Nombre = "Obra Test 3",
            Provincia = "Castellon",
            Municipio = "Castellon",
            FechaInicio = DateTime.Today
        };

        var padre = new Tarea
        {
            Id = 30,
            Nombre = "Tarea padre",
            Proyecto = proyecto,
            ProyectoId = proyecto.Id,
            Estado = EstadoTarea.EnCurso,
            FechaInicio = DateTime.Today
        };

        var subtarea = new Tarea
        {
            Id = 31,
            Nombre = "Subtarea abierta",
            Proyecto = proyecto,
            ProyectoId = proyecto.Id,
            Estado = EstadoTarea.EnCurso,
            FechaInicio = DateTime.Today,
            TareaPadre = padre,
            TareaPadreId = padre.Id
        };

        db.Proyectos.Add(proyecto);
        db.Tareas.AddRange(padre, subtarea);
        await db.SaveChangesAsync();

        var service = new KanbanService(db);

        var accion = () => service.CompletarTareaAsync(padre.Id, "u1", "cierre");

        await accion.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*subtareas no finalizadas*");
    }

    [Fact]
    public async Task CompletarTareaAsync_ConFirmaConjuntaIncompleta_DebeLanzarExcepcion()
    {
        await using var db = TestDbContextFactory.Create();
        var proyecto = new Proyecto
        {
            Id = 4,
            Nombre = "Obra Test 4",
            Provincia = "Murcia",
            Municipio = "Murcia",
            FechaInicio = DateTime.Today
        };

        var usuario1 = new UsuarioObra { Id = "u1", UserName = "u1", NombreCompleto = "Usuario 1", DNI = "00000001A" };
        var usuario2 = new UsuarioObra { Id = "u2", UserName = "u2", NombreCompleto = "Usuario 2", DNI = "00000002B" };

        var tarea = new Tarea
        {
            Id = 40,
            Nombre = "Tarea conjunta",
            Proyecto = proyecto,
            ProyectoId = proyecto.Id,
            Estado = EstadoTarea.EnCurso,
            FechaInicio = DateTime.Today,
            RequiereFirmaConjunta = true,
            UsuariosAsignados = [usuario1, usuario2],
            Firmas = [new FirmaTarea { UsuarioId = "u1", Aprobada = true }]
        };

        db.Users.AddRange(usuario1, usuario2);
        db.Proyectos.Add(proyecto);
        db.Tareas.Add(tarea);
        await db.SaveChangesAsync();

        var service = new KanbanService(db);

        var accion = () => service.CompletarTareaAsync(tarea.Id, "u1", "cierre");

        await accion.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*firma de todos los usuarios asignados*");
    }

    [Fact]
    public async Task FirmarTareaAsync_UsuarioNoAsignado_DebeLanzarExcepcion()
    {
        await using var db = TestDbContextFactory.Create();
        var proyecto = new Proyecto
        {
            Id = 5,
            Nombre = "Obra Test 5",
            Provincia = "Madrid",
            Municipio = "Madrid",
            FechaInicio = DateTime.Today
        };

        var asignado = new UsuarioObra { Id = "u1", UserName = "u1", NombreCompleto = "Asignado", DNI = "00000003C" };
        var externo = new UsuarioObra { Id = "u2", UserName = "u2", NombreCompleto = "Externo", DNI = "00000004D" };

        var tarea = new Tarea
        {
            Id = 50,
            Nombre = "Tarea para firma",
            Proyecto = proyecto,
            ProyectoId = proyecto.Id,
            Estado = EstadoTarea.EnCurso,
            FechaInicio = DateTime.Today,
            RequiereFirmaConjunta = true,
            UsuariosAsignados = [asignado]
        };

        db.Users.AddRange(asignado, externo);
        db.Proyectos.Add(proyecto);
        db.Tareas.Add(tarea);
        await db.SaveChangesAsync();

        var service = new KanbanService(db);

        var accion = () => service.FirmarTareaAsync(tarea.Id, externo.Id, "firma", true);

        await accion.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*usuarios asignados a la tarea pueden firmarla*");
    }
}
