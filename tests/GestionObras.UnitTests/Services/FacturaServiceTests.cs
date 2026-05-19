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
        var proveedorOriginal = new Proveedor { Id = 1, Nombre = "Proveedor Original", Activo = true };
        var proveedorNuevo = new Proveedor { Id = 2, Nombre = "Proveedor Nuevo", Activo = true };
        var proyectoOriginal = new Proyecto
        {
            Id = 1,
            Nombre = "Proyecto Original",
            Provincia = "Valencia",
            Municipio = "Valencia",
            FechaInicio = DateTime.Today
        };
        var proyectoNuevo = new Proyecto
        {
            Id = 2,
            Nombre = "Proyecto Nuevo",
            Provincia = "Alicante",
            Municipio = "Alicante",
            FechaInicio = DateTime.Today.AddDays(1)
        };

        var existente = new Factura
        {
            NumeroFactura = "F-002",
            BaseImponible = 2000m,
            PorcentajeIVA = 21m,
            DescuentoPorcentaje = 0m,
            FechaEmision = DateTime.Today,
            FechaVencimiento = DateTime.Today.AddDays(30),
            Estado = EstadoFactura.Pendiente,
            Concepto = "Factura original",
            ProveedorId = proveedorOriginal.Id,
            ProyectoId = proyectoOriginal.Id,
            MetodoPago = "Transferencia",
            Observaciones = "Observacion original"
        };

        db.Proveedores.AddRange(proveedorOriginal, proveedorNuevo);
        db.Proyectos.AddRange(proyectoOriginal, proyectoNuevo);
        db.Facturas.Add(existente);
        await db.SaveChangesAsync();

        var service = new FacturaService(db);
        await service.GuardarAsync(new Factura
        {
            Id = existente.Id,
            NumeroFactura = "F-002-REV",
            BaseImponible = 1500m,
            PorcentajeIVA = 10m,
            DescuentoPorcentaje = 15m,
            FechaEmision = DateTime.Today.AddDays(5),
            FechaVencimiento = DateTime.Today.AddDays(45),
            FechaPago = DateTime.Today.AddDays(10),
            Estado = EstadoFactura.Pagada,
            Concepto = "Factura corregida",
            ProveedorId = proveedorNuevo.Id,
            ProyectoId = proyectoNuevo.Id,
            MetodoPago = "Tarjeta",
            Observaciones = "Observacion actualizada"
        });

        var actualizada = db.Facturas.Single();
        actualizada.NumeroFactura.Should().Be("F-002-REV");
        actualizada.BaseImponible.Should().Be(1500m);
        actualizada.PorcentajeIVA.Should().Be(10m);
        actualizada.DescuentoPorcentaje.Should().Be(15m);
        actualizada.FechaEmision.Should().Be(DateTime.Today.AddDays(5));
        actualizada.FechaVencimiento.Should().Be(DateTime.Today.AddDays(45));
        actualizada.FechaPago.Should().Be(DateTime.Today.AddDays(10));
        actualizada.Estado.Should().Be(EstadoFactura.Pagada);
        actualizada.Concepto.Should().Be("Factura corregida");
        actualizada.ProveedorId.Should().Be(proveedorNuevo.Id);
        actualizada.ProyectoId.Should().Be(proyectoNuevo.Id);
        actualizada.MetodoPago.Should().Be("Tarjeta");
        actualizada.Observaciones.Should().Be("Observacion actualizada");
        actualizada.IVA.Should().Be(127.5m);
        actualizada.ImporteTotal.Should().Be(1402.5m);
        actualizada.Importe.Should().Be(1402.5m);
    }
}
