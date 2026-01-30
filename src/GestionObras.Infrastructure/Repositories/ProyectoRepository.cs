using GestionObras.Core.Entities;
using GestionObras.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionObras.Infrastructure.Repositories
{
    public class ProyectoRepository : IProyectoRepository
    {
        private readonly GestionObrasDbContext _context;

        public ProyectoRepository(GestionObrasDbContext context)
        {
            _context = context;
        }

        public async Task<List<Proyecto>> GetAllAsync()
        {
            return await _context.Proyectos
                .Include(p => p.Tareas)
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();
        }

        public async Task<Proyecto?> GetByIdAsync(int id)
        {
            return await _context.Proyectos
                .Include(p => p.Tareas)
                .Include(p => p.EmpleadosAsignados)
                .Include(p => p.CarpetaLegal)
                .Include(p => p.Presupuesto)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Proyecto> AddAsync(Proyecto entity)
        {
            _context.Proyectos.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(Proyecto entity)
        {
            _context.Proyectos.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var proyecto = await _context.Proyectos.FindAsync(id);
            if (proyecto != null)
            {
                _context.Proyectos.Remove(proyecto);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Proyecto>> GetProyectosByEstadoAsync(EstadoProyecto estado)
        {
            return await _context.Proyectos
                .Include(p => p.Tareas)
                .Where(p => p.Estado == estado)
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();
        }

        public async Task<List<Proyecto>> GetProyectosByUbicacionAsync(string provincia)
        {
            return await _context.Proyectos
                .Include(p => p.Tareas)
                .Where(p => p.Provincia == provincia)
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();
        }

        public async Task<decimal> GetROIPromedioAsync()
        {
            var proyectos = await _context.Proyectos
                .Include(p => p.Presupuesto)
                .Include(p => p.Tareas)
                .ToListAsync();

            if (!proyectos.Any())
                return 0;

            var rois = proyectos
                .Select(p => p.CalcularROIActual())
                .Where(roi => roi > 0)
                .ToList();

            return rois.Any() ? rois.Average() : 0;
        }

        public async Task<List<Proyecto>> GetProyectosConTareasAsync()
        {
            return await _context.Proyectos
                .Include(p => p.Tareas)
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();
        }
    }
}
