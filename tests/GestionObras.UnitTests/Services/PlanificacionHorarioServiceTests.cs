using FluentAssertions;
using GestionObras.Core.Entities;
using GestionObras.Web.Services;

namespace GestionObras.UnitTests.Services;

public class PlanificacionHorarioServiceTests
{
    [Fact]
    public async Task GenerarHorariosAutomaticosAsync_ProyectoInexistente_DebeDevolverError()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new PlanificacionHorarioService(db);

        var resultado = await service.GenerarHorariosAutomaticosAsync(999);

        resultado.HorariosCreados.Should().Be(0);
        resultado.Alertas.Should().ContainSingle()
            .Which.Should().Contain("No se encontro el proyecto");
    }

    [Fact]
    public async Task GenerarHorariosAutomaticosAsync_ConOperarioYContrato_DebeCrearCincoHorariosSinDeficit()
    {
        await using var db = TestDbContextFactory.Create();
        var operario = new UsuarioObra
        {
            Id = "op1",
            UserName = "op1",
            NormalizedUserName = "OP1",
            NombreCompleto = "Operario Uno",
            TipoUsuario = TipoUsuario.OperarioOficinaT,
            Email = "op1@test.local",
            NormalizedEmail = "OP1@TEST.LOCAL"
        };

        var proyecto = new Proyecto
        {
            Id = 1,
            Nombre = "Obra Test",
            Provincia = "Valencia",
            Municipio = "Valencia",
            FechaInicio = DateTime.Today,
            Tareas =
            [
                new Tarea
                {
                    Id = 1,
                    Nombre = "Albanileria",
                    Estado = EstadoTarea.EnCurso,
                    HorasSemanalesEstimadas = 30m,
                    ResponsableFinalId = operario.Id,
                    ResponsableFinal = operario
                }
            ]
        };

        db.Users.Add(operario);
        db.Proyectos.Add(proyecto);
        db.Contratos.Add(new Contrato
        {
            UsuarioId = operario.Id,
            Usuario = operario,
            Activo = true,
            HorasSemanales = 35,
            FechaInicio = DateOnly.FromDateTime(DateTime.Today),
            TipoContrato = TipoContrato.Indefinido
        });
        await db.SaveChangesAsync();

        var service = new PlanificacionHorarioService(db);
        var resultado = await service.GenerarHorariosAutomaticosAsync(proyecto.Id, new DateOnly(2026, 5, 4));

        resultado.HorariosCreados.Should().Be(5);
        resultado.HorasSemanalesProyecto.Should().Be(30m);
        resultado.HorasSemanalesAsignadas.Should().Be(30m);
        resultado.HorasSemanalesDeficit.Should().Be(0m);
        db.HorariosAsignados.Should().HaveCount(5);
        db.HorariosAsignados.Select(h => h.HorasPrevistas).Should().OnlyContain(h => h <= 8d);
    }

    [Fact]
    public async Task GenerarHorariosAutomaticosAsync_ConResponsableNoOperativo_DebeGenerarDeficitYAlerta()
    {
        await using var db = TestDbContextFactory.Create();
        var jefe = new UsuarioObra
        {
            Id = "jefe1",
            UserName = "jefe1",
            NormalizedUserName = "JEFE1",
            NombreCompleto = "Jefe de Obra",
            TipoUsuario = TipoUsuario.JefeObra,
            Email = "jefe@test.local",
            NormalizedEmail = "JEFE@TEST.LOCAL"
        };

        db.Users.Add(jefe);
        db.Proyectos.Add(new Proyecto
        {
            Id = 2,
            Nombre = "Obra 2",
            Provincia = "Castellon",
            Municipio = "Castellon",
            FechaInicio = DateTime.Today,
            Tareas =
            [
                new Tarea
                {
                    Id = 2,
                    Nombre = "Supervision",
                    Estado = EstadoTarea.EnCurso,
                    HorasSemanalesEstimadas = 12m,
                    ResponsableFinalId = jefe.Id,
                    ResponsableFinal = jefe
                }
            ]
        });
        await db.SaveChangesAsync();

        var service = new PlanificacionHorarioService(db);
        var resultado = await service.GenerarHorariosAutomaticosAsync(2);

        resultado.HorasSemanalesDeficit.Should().Be(12m);
        resultado.HorariosCreados.Should().Be(0);
        resultado.Alertas.Should().Contain(a => a.Contains("no es un perfil operativo"));
    }
}
