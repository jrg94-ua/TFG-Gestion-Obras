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
        // Recalcular importes
        factura.IVA = factura.BaseImponible * (factura.PorcentajeIVA / 100);
        factura.ImporteTotal = factura.BaseImponible + factura.IVA;
        factura.Importe = factura.ImporteTotal;

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
                existente.Concepto = factura.Concepto;
                existente.FechaEmision = factura.FechaEmision;
                existente.FechaVencimiento = factura.FechaVencimiento;
                existente.FechaPago = factura.FechaPago;
                existente.BaseImponible = factura.BaseImponible;
                existente.PorcentajeIVA = factura.PorcentajeIVA;
                existente.IVA = factura.IVA;
                existente.ImporteTotal = factura.ImporteTotal;
                existente.Importe = factura.ImporteTotal;
                existente.Estado = factura.Estado;
                existente.NombreProyecto = factura.NombreProyecto;
                existente.MetodoPago = factura.MetodoPago;
                existente.Observaciones = factura.Observaciones;
                existente.ProveedorId = factura.ProveedorId;
                existente.ProyectoId = factura.ProyectoId;
            }
        }

        await _db.SaveChangesAsync();
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
