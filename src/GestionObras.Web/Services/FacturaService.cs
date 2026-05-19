using GestionObras.Core.Entities;
using GestionObras.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionObras.Web.Services;

public class FacturaService
{
    private readonly GestionObrasDbContext _db;

    public FacturaService(GestionObrasDbContext db)
    {
        _db = db;
    }

    public async Task<List<Factura>> ObtenerTodasAsync()
    {
        return await _db.Facturas
            .Include(f => f.Proveedor)
            .Include(f => f.Proyecto)
            .OrderByDescending(f => f.FechaEmision)
            .ToListAsync();
    }

    public async Task<List<Proveedor>> ObtenerProveedoresAsync()
    {
        return await _db.Proveedores
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<List<Proyecto>> ObtenerProyectosAsync()
    {
        return await _db.Proyectos
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task GuardarAsync(Factura factura)
    {
        RecalcularImportes(factura);

        if (factura.Id == 0)
        {
            _db.Facturas.Add(factura);
        }
        else
        {
            var existente = await _db.Facturas.FindAsync(factura.Id);
            if (existente != null)
            {
                existente.NumeroFactura = factura.NumeroFactura;
                existente.FechaEmision = factura.FechaEmision;
                existente.FechaVencimiento = factura.FechaVencimiento;
                existente.FechaPago = factura.FechaPago;
                existente.Estado = factura.Estado;
                existente.Concepto = factura.Concepto;
                existente.BaseImponible = factura.BaseImponible;
                existente.PorcentajeIVA = factura.PorcentajeIVA;
                existente.DescuentoPorcentaje = factura.DescuentoPorcentaje;
                existente.NombreProyecto = factura.NombreProyecto;
                existente.MetodoPago = factura.MetodoPago;
                existente.Observaciones = factura.Observaciones;
                existente.ProyectoId = factura.ProyectoId;
                existente.TareaId = factura.TareaId;
                existente.ProveedorId = factura.ProveedorId;
                RecalcularImportes(existente);
            }
        }

        await _db.SaveChangesAsync();
    }

    private static void RecalcularImportes(Factura factura)
    {
        var descuento = factura.BaseImponible * (factura.DescuentoPorcentaje / 100);
        var baseNeta = factura.BaseImponible - descuento;

        factura.IVA = baseNeta * (factura.PorcentajeIVA / 100);
        factura.ImporteTotal = baseNeta + factura.IVA;
        factura.Importe = factura.ImporteTotal;
    }

    public async Task EliminarAsync(int id)
    {
        var factura = await _db.Facturas.FindAsync(id);
        if (factura != null)
        {
            _db.Facturas.Remove(factura);
            await _db.SaveChangesAsync();
        }
    }
}
