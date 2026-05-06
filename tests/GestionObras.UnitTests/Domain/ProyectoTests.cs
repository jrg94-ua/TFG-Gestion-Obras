using FluentAssertions;
using GestionObras.Core.Entities;

namespace GestionObras.UnitTests.Domain;

public class ProyectoTests
{
    [Fact]
    public void CalcularROIActual_SinPresupuesto_DebeDevolverCero()
    {
        var proyecto = new Proyecto
        {
            Tareas =
            [
                new Tarea { CostesReales = 1000m }
            ]
        };

        proyecto.CalcularROIActual().Should().Be(0m);
    }

    [Fact]
    public void CalcularROIActual_SinCostesReales_DebeDevolverCero()
    {
        var proyecto = new Proyecto
        {
            Presupuesto = new Presupuesto { Total = 100000m },
            Tareas =
            [
                new Tarea { CostesReales = 0m }
            ]
        };

        proyecto.CalcularROIActual().Should().Be(0m);
    }

    [Fact]
    public void CalcularROIActual_ConPresupuestoYCostes_DebeCalcularCorrectamente()
    {
        var proyecto = new Proyecto
        {
            Presupuesto = new Presupuesto { Total = 100000m },
            Tareas =
            [
                new Tarea { CostesReales = 30000m },
                new Tarea { CostesReales = 50000m }
            ]
        };

        proyecto.CalcularROIActual().Should().Be(25m);
    }
}
