using FluentAssertions;
using GestionObras.Core.Entities;
using GestionObras.Web.Services;

namespace GestionObras.UnitTests.Services;

public class FacturaServiceTests
{
    [Fact]
    public async Task GuardarAsync_NuevaFactura_DebeRecalcularImportesYPersistir()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new FacturaService(db);

        var factura = new Factura
        {
            NumeroFactura = "F-001",
            BaseImponible = 1000m,
            PorcentajeIVA = 21m,
            DescuentoPorcentaje = 10m,
            FechaEmision = DateTime.Today,
            FechaVencimiento = DateTime.Today.AddDays(30)
        };

        await service.GuardarAsync(factura);

        var guardada = db.Facturas.Single();
        guardada.IVA.Should().Be(189m);
        guardada.ImporteTotal.Should().Be(1089m);
        guardada.Importe.Should().Be(1089m);
    }

    [Fact]
    public async Task GuardarAsync_FacturaExistente_DebeActualizarDescuentoYRecalcular()
    {
        await using var db = TestDbContextFactory.Create();
        var existente = new Factura
        {
            NumeroFactura = "F-002",
            BaseImponible = 2000m,
            PorcentajeIVA = 21m,
            DescuentoPorcentaje = 0m,
            FechaEmision = DateTime.Today,
            FechaVencimiento = DateTime.Today.AddDays(30)
        };

        db.Facturas.Add(existente);
        await db.SaveChangesAsync();

        var service = new FacturaService(db);
        await service.GuardarAsync(new Factura
        {
            Id = existente.Id,
            DescuentoPorcentaje = 15m
        });

        var actualizada = db.Facturas.Single();
        actualizada.DescuentoPorcentaje.Should().Be(15m);
        actualizada.IVA.Should().Be(357m);
        actualizada.ImporteTotal.Should().Be(2057m);
    }
}
