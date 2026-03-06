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
            .OrderBy(m => m.Nombre)
            .ToListAsync();
    }

    public async Task GuardarAsync(Material material)
    {
        if (material.Id == 0)
        {
            _db.Materiales.Add(material);
        }
        else
        {
            var existente = await _db.Materiales.FindAsync(material.Id);
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
