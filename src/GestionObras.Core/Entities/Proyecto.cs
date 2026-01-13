using System;
using System.Collections.Generic;

namespace GestionObras.Core.Entities
{
    /// <summary>
    /// Entidad principal que representa un proyecto de construcción
    /// </summary>
    public class Proyecto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public EstadoProyecto Estado { get; set; }
        
        // Ubicación y contexto normativo
        public string Provincia { get; set; } = string.Empty;
        public string Municipio { get; set; } = string.Empty;
        public TipoSuelo TipoSuelo { get; set; }
        public ZonaClimatica ZonaClimatica { get; set; }
        
        // Relaciones
        public List<Tarea> Tareas { get; set; } = new();
        public List<Empleado> EmpleadosAsignados { get; set; } = new();
        public CarpetaLegal? CarpetaLegal { get; set; }
        public Presupuesto? Presupuesto { get; set; }
        
        /// <summary>
        /// Calcula el ROI actual del proyecto
        /// </summary>
        public decimal CalcularROIActual()
        {
            if (Presupuesto == null) return 0;
            
            var costesReales = ObtenerCostesReales();
            if (costesReales == 0) return 0;
            
            var beneficioProyectado = Presupuesto.Total - costesReales;
            return (beneficioProyectado / costesReales) * 100;
        }
        
        /// <summary>
        /// Obtiene los costes reales acumulados del proyecto
        /// </summary>
        public decimal ObtenerCostesReales()
        {
            decimal total = 0;
            foreach (var tarea in Tareas)
            {
                total += tarea.CostesReales;
            }
            return total;
        }
        
        /// <summary>
        /// Verifica si una tarea puede iniciarse (validaciones de PRL, materiales, etc.)
        /// </summary>
        public bool PuedeIniciarTarea(Tarea tarea)
        {
            // Verificar que todos los empleados asignados tengan PRL vigente
            foreach (var empleado in tarea.Responsables)
            {
                if (!empleado.TienePRLVigente())
                    return false;
            }
            
            // Verificar que los materiales estén disponibles
            // (lógica adicional aquí)
            
            return true;
        }
    }
    
    public enum EstadoProyecto
    {
        Planificacion,
        EnCurso,
        Bloqueado,
        Finalizado,
        Cancelado
    }
    
    public enum TipoSuelo
    {
        Urbano,
        Rustico
    }
    
    public enum ZonaClimatica
    {
        A3, // Cádiz, Málaga
        B3, // Valencia, Murcia
        C3, // Madrid
        D3, // Zaragoza
        E1  // Burgos, León
    }
}
