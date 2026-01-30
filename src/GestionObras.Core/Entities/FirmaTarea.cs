using System;

namespace GestionObras.Core.Entities
{
    /// <summary>
    /// Representa la firma de un usuario en una tarea
    /// Para tareas conjuntas, todos los usuarios asignados deben firmar
    /// </summary>
    public class FirmaTarea
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Tarea que se está firmando
        /// </summary>
        public int TareaId { get; set; }
        public Tarea Tarea { get; set; } = null!;
        
        /// <summary>
        /// Usuario que firma
        /// </summary>
        public string UsuarioId { get; set; } = string.Empty;
        public UsuarioObra Usuario { get; set; } = null!;
        
        /// <summary>
        /// Fecha y hora de la firma
        /// </summary>
        public DateTime FechaFirma { get; set; }
        
        /// <summary>
        /// Observaciones del usuario al firmar
        /// </summary>
        public string? Observaciones { get; set; }
        
        /// <summary>
        /// Indica si el usuario aprueba o rechaza la tarea
        /// </summary>
        public bool Aprobada { get; set; }
        
        /// <summary>
        /// Motivo de rechazo si no fue aprobada
        /// </summary>
        public string? MotivoRechazo { get; set; }
    }
}
