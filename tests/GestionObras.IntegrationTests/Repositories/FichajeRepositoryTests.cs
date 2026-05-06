using FluentAssertions;
using GestionObras.Core.Entities;
using GestionObras.Infrastructure.Repositories;

namespace GestionObras.IntegrationTests.Repositories;

public class FichajeRepositoryTests
{
    [Fact]
    public async Task GetFichajeAbiertoAsync_DebeDevolverSoloElFichajeSinSalida()
    {
        await using var factory = new SqliteTestDbContextFactory();
        await using var db = factory.Create();

        var usuario = new UsuarioObra
        {
            Id = "op1",
            UserName = "op1",
            NormalizedUserName = "OP1",
            Email = "op1@test.local",
            NormalizedEmail = "OP1@TEST.LOCAL",
            DNI = "00000001A",
            NombreCompleto = "Operario Uno",
            TipoUsuario = TipoUsuario.Operario
        };
        var proyecto = new Proyecto
        {
            Id = 1,
            Nombre = "Obra A",
            Provincia = "Valencia",
            Municipio = "Valencia",
            FechaInicio = DateTime.Today
        };

        db.Users.Add(usuario);
        db.Proyectos.Add(proyecto);
        db.RegistrosFichaje.AddRange(
            new RegistroFichaje
            {
                UsuarioId = usuario.Id,
                Usuario = usuario,
                ProyectoId = proyecto.Id,
                Proyecto = proyecto,
                Fecha = new DateOnly(2026, 5, 4),
                HoraEntrada = new DateTime(2026, 5, 4, 8, 0, 0),
                HoraSalida = new DateTime(2026, 5, 4, 15, 0, 0)
            },
            new RegistroFichaje
            {
                UsuarioId = usuario.Id,
                Usuario = usuario,
                ProyectoId = proyecto.Id,
                Proyecto = proyecto,
                Fecha = new DateOnly(2026, 5, 5),
                HoraEntrada = new DateTime(2026, 5, 5, 8, 0, 0)
            });
        await db.SaveChangesAsync();

        var repository = new FichajeRepository(db);
        var abierto = await repository.GetFichajeAbiertoAsync(usuario.Id, new DateOnly(2026, 5, 5));

        abierto.Should().NotBeNull();
        abierto!.HoraSalida.Should().BeNull();
        abierto.Fecha.Should().Be(new DateOnly(2026, 5, 5));
    }

    [Fact]
    public async Task GetHorariosActivosByProyectoAsync_DebeFiltrarPorProyectoYOrdenarPorUsuarioYDia()
    {
        await using var factory = new SqliteTestDbContextFactory();
        await using var db = factory.Create();

        var usuarioA = new UsuarioObra
        {
            Id = "uA",
            UserName = "ua",
            NormalizedUserName = "UA",
            Email = "ua@test.local",
            NormalizedEmail = "UA@TEST.LOCAL",
            DNI = "00000002B",
            NombreCompleto = "Ana Alba",
            TipoUsuario = TipoUsuario.Operario
        };
        var usuarioB = new UsuarioObra
        {
            Id = "uB",
            UserName = "ub",
            NormalizedUserName = "UB",
            Email = "ub@test.local",
            NormalizedEmail = "UB@TEST.LOCAL",
            DNI = "00000003C",
            NombreCompleto = "Berto Bravo",
            TipoUsuario = TipoUsuario.Operario
        };
        var proyectoA = new Proyecto
        {
            Id = 10,
            Nombre = "Obra Alfa",
            Provincia = "Valencia",
            Municipio = "Valencia",
            FechaInicio = DateTime.Today
        };
        var proyectoB = new Proyecto
        {
            Id = 11,
            Nombre = "Obra Beta",
            Provincia = "Alicante",
            Municipio = "Alicante",
            FechaInicio = DateTime.Today
        };

        db.Users.AddRange(usuarioA, usuarioB);
        db.Proyectos.AddRange(proyectoA, proyectoB);
        db.HorariosAsignados.AddRange(
            new HorarioAsignado
            {
                UsuarioId = usuarioB.Id,
                Usuario = usuarioB,
                ProyectoId = proyectoA.Id,
                Proyecto = proyectoA,
                DiaSemana = DiaSemana.Martes,
                HoraEntrada = new TimeOnly(8, 0),
                HoraSalida = new TimeOnly(14, 0),
                TipoTurno = (TipoTurno)0,
                VigenteDesde = new DateOnly(2026, 5, 4),
                Activo = true
            },
            new HorarioAsignado
            {
                UsuarioId = usuarioA.Id,
                Usuario = usuarioA,
                ProyectoId = proyectoA.Id,
                Proyecto = proyectoA,
                DiaSemana = DiaSemana.Lunes,
                HoraEntrada = new TimeOnly(8, 0),
                HoraSalida = new TimeOnly(14, 0),
                TipoTurno = (TipoTurno)0,
                VigenteDesde = new DateOnly(2026, 5, 4),
                Activo = true
            },
            new HorarioAsignado
            {
                UsuarioId = usuarioA.Id,
                Usuario = usuarioA,
                ProyectoId = proyectoB.Id,
                Proyecto = proyectoB,
                DiaSemana = DiaSemana.Lunes,
                HoraEntrada = new TimeOnly(8, 0),
                HoraSalida = new TimeOnly(14, 0),
                TipoTurno = (TipoTurno)0,
                VigenteDesde = new DateOnly(2026, 5, 4),
                Activo = true
            });
        await db.SaveChangesAsync();

        var repository = new FichajeRepository(db);
        var horarios = await repository.GetHorariosActivosByProyectoAsync(proyectoA.Id);

        horarios.Should().HaveCount(2);
        horarios.Select(h => h.Usuario.NombreCompleto).Should().ContainInOrder("Ana Alba", "Berto Bravo");
        horarios.Should().OnlyContain(h => h.ProyectoId == proyectoA.Id);
    }

    [Fact]
    public async Task DeleteHorarioAsync_DebeEliminarElHorarioPersistido()
    {
        await using var factory = new SqliteTestDbContextFactory();
        await using var db = factory.Create();

        var usuario = new UsuarioObra
        {
            Id = "u1",
            UserName = "u1",
            NormalizedUserName = "U1",
            Email = "u1@test.local",
            NormalizedEmail = "U1@TEST.LOCAL",
            DNI = "00000004D",
            NombreCompleto = "Usuario Uno",
            TipoUsuario = TipoUsuario.Operario
        };

        db.Users.Add(usuario);
        db.HorariosAsignados.Add(new HorarioAsignado
        {
            UsuarioId = usuario.Id,
            Usuario = usuario,
            DiaSemana = DiaSemana.Lunes,
            HoraEntrada = new TimeOnly(8, 0),
            HoraSalida = new TimeOnly(14, 0),
            TipoTurno = (TipoTurno)0,
            VigenteDesde = new DateOnly(2026, 5, 4),
            Activo = true
        });
        await db.SaveChangesAsync();
        var horarioId = db.HorariosAsignados.Single().Id;

        var repository = new FichajeRepository(db);
        await repository.DeleteHorarioAsync(horarioId);

        db.HorariosAsignados.Should().BeEmpty();
    }
}
