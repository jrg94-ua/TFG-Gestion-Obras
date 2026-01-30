using GestionObras.Core.Entities;
using GestionObras.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionObras.Infrastructure.Repositories
{
    public class TareaRepository : ITareaRepository
    {
        private readonly GestionObrasDbContext _context;

        public TareaRepository(GestionObrasDbContext context)
        {
            _context = context;
        }

        public async Task<List<Tarea>> GetAllAsync()
        {
            return await _context.Tareas
                .Include(t => t.Proyecto)
                .Include(t => t.Bloqueo)
                .OrderBy(t => t.FechaInicio)
                .ToListAsync();
        }

        public async Task<Tarea?> GetByIdAsync(int id)
        {
            return await _context.Tareas
                .Include(t => t.Proyecto)
                .Include(t => t.MaterialesNecesarios)
                .Include(t => t.Responsables)
                .Include(t => t.Bloqueo)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Tarea> AddAsync(Tarea entity)
        {
            _context.Tareas.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(Tarea entity)
        {
            _context.Tareas.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea != null)
            {
                _context.Tareas.Remove(tarea);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Tarea>> GetTareasByProyectoAsync(int proyectoId)
        {
            return await _context.Tareas
                .Include(t => t.Bloqueo)
                .Include(t => t.Responsables)
                .Where(t => t.ProyectoId == proyectoId)
                .OrderBy(t => t.FechaInicio)
                .ToListAsync();
        }

        public async Task<List<Tarea>> GetTareasByEstadoAsync(EstadoTarea estado)
        {
            return await _context.Tareas
                .Include(t => t.Proyecto)
                .Include(t => t.Bloqueo)
                .Where(t => t.Estado == estado)
                .OrderBy(t => t.FechaInicio)
                .ToListAsync();
        }

        public async Task<int> GetTareasPendientesCountAsync()
        {
            return await _context.Tareas
                .CountAsync(t => t.Estado == EstadoTarea.Pendiente || t.Estado == EstadoTarea.EnCurso);
        }
    }
}
