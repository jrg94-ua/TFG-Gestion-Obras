using GestionObras.Core.Entities;
using GestionObras.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionObras.Infrastructure.Repositories
{
    public class EmpleadoRepository : IEmpleadoRepository
    {
        private readonly GestionObrasDbContext _context;

        public EmpleadoRepository(GestionObrasDbContext context)
        {
            _context = context;
        }

        public async Task<List<Empleado>> GetAllAsync()
        {
            return await _context.Empleados
                .Include(e => e.CursosPRL)
                .Include(e => e.ProyectosAsignados)
                .OrderBy(e => e.Nombre)
                .ThenBy(e => e.Apellidos)
                .ToListAsync();
        }

        public async Task<Empleado?> GetByIdAsync(int id)
        {
            return await _context.Empleados
                .Include(e => e.CursosPRL)
                .Include(e => e.ProyectosAsignados)
                .Include(e => e.Fichajes)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Empleado> AddAsync(Empleado entity)
        {
            _context.Empleados.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(Empleado entity)
        {
            _context.Empleados.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado != null)
            {
                _context.Empleados.Remove(empleado);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Empleado?> GetByDniAsync(string dni)
        {
            return await _context.Empleados
                .Include(e => e.CursosPRL)
                .FirstOrDefaultAsync(e => e.DNI == dni);
        }

        public async Task<List<Empleado>> GetEmpleadosActivosAsync()
        {
            return await _context.Empleados
                .Include(e => e.CursosPRL)
                .Where(e => e.ProyectosAsignados.Any())
                .OrderBy(e => e.Nombre)
                .ThenBy(e => e.Apellidos)
                .ToListAsync();
        }
    }
}
