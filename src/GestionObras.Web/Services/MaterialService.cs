using GestionObras.Core.Entities;
using GestionObras.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionObras.Web.Services;

public class MaterialService
{
    private readonly GestionObrasDbContext _db;

    public MaterialService(GestionObrasDbContext db)
    {
        _db = db;
    }

    public async Task<List<Material>> ObtenerTodosAsync()
    {
        return await _db.Materiales
            .Include(m => m.Proveedor)
            .Include(m => m.Proveedores)
            .OrderBy(m => m.Nombre)
            .ToListAsync();
    }

    public async Task GuardarAsync(Material material, IEnumerable<int>? proveedorIds = null)
    {
        var idsProveedores = (proveedorIds ?? material.Proveedores.Select(p => p.Id))
            .Distinct()
            .ToList();

        if (material.Id == 0)
        {
            if (material.ProveedorId == null && idsProveedores.Count > 0)
            {
                material.ProveedorId = idsProveedores[0];
            }

            if (idsProveedores.Count > 0)
            {
                material.Proveedores = await _db.Proveedores
                    .Where(p => idsProveedores.Contains(p.Id))
                    .ToListAsync();
            }

            _db.Materiales.Add(material);
        }
        else
        {
            var existente = await _db.Materiales
                .Include(m => m.Proveedores)
                .FirstOrDefaultAsync(m => m.Id == material.Id);
            if (existente != null)
            {
                existente.Codigo = material.Codigo;
                existente.Nombre = material.Nombre;
                existente.Descripcion = material.Descripcion;
                existente.Categoria = material.Categoria;
                existente.UnidadMedida = material.UnidadMedida;
                existente.PrecioUnitario = material.PrecioUnitario;
                existente.StockDisponible = material.StockDisponible;
                existente.StockMinimo = material.StockMinimo;
                existente.Activo = material.Activo;
                existente.ProveedorId = material.ProveedorId ?? (idsProveedores.Count > 0 ? idsProveedores[0] : (int?)null);

                var proveedoresDeseados = idsProveedores.Count == 0
                    ? new List<Proveedor>()
                    : await _db.Proveedores.Where(p => idsProveedores.Contains(p.Id)).ToListAsync();

                existente.Proveedores.Clear();
                foreach (var proveedor in proveedoresDeseados)
                {
                    existente.Proveedores.Add(proveedor);
                }
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task EliminarAsync(int id)
    {
        var material = await _db.Materiales.FindAsync(id);
        if (material != null)
        {
            _db.Materiales.Remove(material);
            await _db.SaveChangesAsync();
        }
    }
}
