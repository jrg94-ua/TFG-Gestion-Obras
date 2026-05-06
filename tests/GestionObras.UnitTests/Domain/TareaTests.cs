using FluentAssertions;
using GestionObras.Core.Entities;

namespace GestionObras.UnitTests.Domain;

public class TareaTests
{
    [Fact]
    public void TodosHanFirmado_SinFirmaConjunta_DebeDevolverTrue()
    {
        var tarea = new Tarea
        {
            RequiereFirmaConjunta = false
        };

        tarea.TodosHanFirmado().Should().BeTrue();
    }

    [Fact]
    public void TodosHanFirmado_CuandoFaltaUnaFirma_DebeDevolverFalse()
    {
        var tarea = new Tarea
        {
            RequiereFirmaConjunta = true,
            UsuariosAsignados =
            [
                new UsuarioObra { Id = "u1", NombreCompleto = "Operario 1" },
                new UsuarioObra { Id = "u2", NombreCompleto = "Operario 2" }
            ],
            Firmas =
            [
                new FirmaTarea { UsuarioId = "u1", Aprobada = true }
            ]
        };

        tarea.TodosHanFirmado().Should().BeFalse();
    }

    [Fact]
    public void TodosHanFirmado_CuandoTodosAprueban_DebeDevolverTrue()
    {
        var tarea = new Tarea
        {
            RequiereFirmaConjunta = true,
            UsuariosAsignados =
            [
                new UsuarioObra { Id = "u1" },
                new UsuarioObra { Id = "u2" }
            ],
            Firmas =
            [
                new FirmaTarea { UsuarioId = "u1", Aprobada = true },
                new FirmaTarea { UsuarioId = "u2", Aprobada = true }
            ]
        };

        tarea.TodosHanFirmado().Should().BeTrue();
    }

    [Fact]
    public void UsuarioHaFirmado_CuandoExisteFirmaDelUsuario_DebeDevolverTrue()
    {
        var tarea = new Tarea
        {
            Firmas =
            [
                new FirmaTarea { UsuarioId = "u1", Aprobada = true }
            ]
        };

        tarea.UsuarioHaFirmado("u1").Should().BeTrue();
    }
}
