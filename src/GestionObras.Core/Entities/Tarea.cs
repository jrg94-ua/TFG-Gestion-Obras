using System;
using System.Collections.Generic;

namespace GestionObras.Core.Entities
{
    /// <summary>
    /// Representa una tarea dentro de un proyecto de construcción
    /// </summary>
    public class Tarea
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public EstadoTarea Estado { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        
        // Presupuesto y costes
        public decimal PresupuestoEstimado { get; set; }
        public decimal CostesReales { get; set; }
        
        // Relaciones
        public int ProyectoId { get; set; }
        public Proyecto Proyecto { get; set; } = null!;
        
        // Jerarquía de tareas
        public int? TareaPadreId { get; set; }
        public Tarea? TareaPadre { get; set; }
        public List<Tarea> SubTareas { get; set; } = new();
        public List<Tarea> Predecesoras { get; set; } = new();
        public List<Tarea> Dependientes { get; set; } = new();
        public int Nivel { get; set; } = 0; // 0 = tarea raíz, 1+ = subtarea
        public PrioridadTarea Prioridad { get; set; } = PrioridadTarea.Media;
        
        public List<Material> MaterialesNecesarios { get; set; } = new();
        public List<Empleado> Responsables { get; set; } = new();
        public List<UsuarioObra> UsuariosAsignados { get; set; } = new();
        public BloqueoTarea? Bloqueo { get; set; }
        public List<Factura> Facturas { get; set; } = new();
        
        // Documentos adjuntos
        public List<DocumentoTarea> Documentos { get; set; } = new();
        
        // Sistema de firmas para tareas conjuntas
        /// <summary>
        /// Indica si la tarea requiere que todos los usuarios asignados firmen para completarla
        /// </summary>
        public bool RequiereFirmaConjunta { get; set; } = false;
        
        /// <summary>
        /// Firmas de los usuarios en la tarea
        /// </summary>
        public List<FirmaTarea> Firmas { get; set; } = new();
        
        // Control de finalización y firma
        /// <summary>
        /// Usuario que completó la tarea
        /// </summary>
        public string? CompletadaPorId { get; set; }
        public UsuarioObra? CompletadaPor { get; set; }
        
        /// <summary>
        /// Fecha en que se marcó como completada
        /// </summary>
        public DateTime? FechaFinalizacion { get; set; }
        
        /// <summary>
        /// Observaciones o comentarios al completar la tarea
        /// </summary>
        public string? ObservacionesFinalizacion { get; set; }
        
        /// <summary>
        /// Verifica si todos los usuarios asignados han firmado la tarea
        /// </summary>
        public bool TodosHanFirmado()
        {
            if (!RequiereFirmaConjunta || UsuariosAsignados == null || !UsuariosAsignados.Any())
                return true;
                
            var usuariosAsignadosIds = UsuariosAsignados.Select(u => u.Id).ToHashSet();
            var usuariosFirmadosIds = Firmas.Where(f => f.Aprobada).Select(f => f.UsuarioId).ToHashSet();
            
            return usuariosAsignadosIds.All(id => usuariosFirmadosIds.Contains(id));
        }
        
        /// <summary>
        /// Verifica si un usuario específico ya firmó la tarea
        /// </summary>
        public bool UsuarioHaFirmado(string usuarioId)
        {
            return Firmas.Any(f => f.UsuarioId == usuarioId);
        }
    }
    
    public enum EstadoTarea
    {
        Pendiente,
        EnCurso,
        Bloqueado,
        Finalizado
    }
    
    /// <summary>
    /// Representa un bloqueo en una tarea (requisito funcional RF-07)
    /// </summary>
    public class BloqueoTarea
    {
        public int Id { get; set; }
        public int TareaId { get; set; }
        public Tarea Tarea { get; set; } = null!;
        public TipoBloqueo Tipo { get; set; }
        public string JustificacionTecnica { get; set; } = string.Empty;
        public DateTime FechaBloqueo { get; set; }
        public DateTime? FechaResolucion { get; set; }
    }
    
    public enum TipoBloqueo
    {
        FaltaMaterial,
        ErrorEjecucion,
        IncidenciaNormativa,
        ClimatologiaAdversa,
        Otro
    }
    
    public enum PrioridadTarea
    {
        Baja,
        Media,
        Alta,
        Critica
    }
}
