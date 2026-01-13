using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GestionObras.Core.Entities;

namespace GestionObras.Core.Interfaces
{
    /// <summary>
    /// Repositorio base con operaciones CRUD genéricas
    /// </summary>
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }
    
    /// <summary>
    /// Repositorio específico para Proyectos
    /// </summary>
    public interface IProyectoRepository : IRepository<Proyecto>
    {
        Task<IEnumerable<Proyecto>> GetProyectosEnCursoAsync();
        Task<IEnumerable<Proyecto>> GetProyectosPorUbicacionAsync(string provincia, string municipio);
        Task<Proyecto?> GetProyectoConTareasAsync(int id);
    }
    
    /// <summary>
    /// Repositorio específico para Empleados
    /// </summary>
    public interface IEmpleadoRepository : IRepository<Empleado>
    {
        Task<IEnumerable<Empleado>> GetEmpleadosConPRLVigenteAsync();
        Task<Empleado?> GetEmpleadoPorDNIAsync(string dni);
    }
    
    /// <summary>
    /// Repositorio específico para Materiales
    /// </summary>
    public interface IMaterialRepository : IRepository<Material>
    {
        Task<IEnumerable<Material>> GetMaterialesDisponiblesAsync();
        Task<Material?> GetMaterialPorCodigoAsync(string codigo);
    }
    
    /// <summary>
    /// Repositorio específico para Tareas
    /// </summary>
    public interface ITareaRepository : IRepository<Tarea>
    {
        Task<IEnumerable<Tarea>> GetTareasPorProyectoAsync(int proyectoId);
        Task<IEnumerable<Tarea>> GetTareasBloqueadasAsync();
    }
}
