using GestionObras.Core.Entities;
using GestionObras.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionObras.Web.Services;

public class PresupuestoService
{
    private readonly GestionObrasDbContext _db;

    public PresupuestoService(GestionObrasDbContext db)
    {
        _db = db;
    }

    public async Task<List<Presupuesto>> ObtenerTodosAsync()
    {
        return await _db.Presupuestos
            .Include(p => p.Proyecto)
            .OrderByDescending(p => p.FechaElaboracion)
            .ToListAsync();
    }

    public async Task<List<Proyecto>> ObtenerProyectosAsync()
    {
        return await _db.Proyectos
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task GuardarAsync(Presupuesto presupuesto)
    {
        if (presupuesto.Id == 0)
        {
            _db.Presupuestos.Add(presupuesto);
        }
        else
        {
            var existente = await _db.Presupuestos.FindAsync(presupuesto.Id);
            if (existente != null)
            {
                existente.ProyectoId = presupuesto.ProyectoId;
                existente.Total = presupuesto.Total;
                existente.FechaElaboracion = presupuesto.FechaElaboracion;
                existente.Observaciones = presupuesto.Observaciones;
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task EliminarAsync(int id)
    {
        var presupuesto = await _db.Presupuestos.FindAsync(id);
        if (presupuesto != null)
        {
            _db.Presupuestos.Remove(presupuesto);
            await _db.SaveChangesAsync();
        }
    }
}
