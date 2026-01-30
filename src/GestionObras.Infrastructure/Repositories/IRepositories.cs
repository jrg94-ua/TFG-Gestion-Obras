using GestionObras.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionObras.Infrastructure.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }

    public interface IProyectoRepository : IRepository<Proyecto>
    {
        Task<List<Proyecto>> GetProyectosByEstadoAsync(EstadoProyecto estado);
        Task<List<Proyecto>> GetProyectosByUbicacionAsync(string provincia);
        Task<decimal> GetROIPromedioAsync();
        Task<List<Proyecto>> GetProyectosConTareasAsync();
    }

    public interface ITareaRepository : IRepository<Tarea>
    {
        Task<List<Tarea>> GetTareasByProyectoAsync(int proyectoId);
        Task<List<Tarea>> GetTareasByEstadoAsync(EstadoTarea estado);
        Task<int> GetTareasPendientesCountAsync();
    }

    public interface IEmpleadoRepository : IRepository<Empleado>
    {
        Task<Empleado?> GetByDniAsync(string dni);
        Task<List<Empleado>> GetEmpleadosActivosAsync();
    }
}
