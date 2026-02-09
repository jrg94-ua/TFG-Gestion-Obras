using System;
using System.ComponentModel.DataAnnotations;

namespace GestionObras.Core.Entities
{
    /// <summary>
    /// Representa una solicitud de materiales realizada por un Jefe de Obra
    /// que debe ser aprobada por el Administrador/Presupuestos
    /// </summary>
    public class SolicitudMaterial
    {
        public int Id { get; set; }

        // Material solicitado
        [Required]
        public int MaterialId { get; set; }
        public Material Material { get; set; } = null!;

        // Proyecto al que se destina
        [Required]
        public int ProyectoId { get; set; }
        public Proyecto Proyecto { get; set; } = null!;

        // Cantidad solicitada
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor que 0")]
        public decimal CantidadSolicitada { get; set; }

        // Justificación de la solicitud
        [Required(ErrorMessage = "Debe indicar la justificación de la solicitud")]
        [StringLength(1000)]
        public string Justificacion { get; set; } = string.Empty;

        // Usuario que solicita (Jefe de Obra)
        [Required]
        public string SolicitadoPorId { get; set; } = string.Empty;
        public UsuarioObra SolicitadoPor { get; set; } = null!;

        // Fecha de solicitud
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        // Estado de la solicitud
        public EstadoSolicitudMaterial Estado { get; set; } = EstadoSolicitudMaterial.Pendiente;

        // Respuesta del administrador
        public string? RevisadoPorId { get; set; }
        public UsuarioObra? RevisadoPor { get; set; }

        public DateTime? FechaRespuesta { get; set; }

        [StringLength(1000)]
        public string? ObservacionesAdmin { get; set; }

        // Prioridad de la solicitud
        public PrioridadSolicitud Prioridad { get; set; } = PrioridadSolicitud.Media;

        // Fecha en que se necesita el material
        public DateTime? FechaNecesaria { get; set; }
    }

    public enum EstadoSolicitudMaterial
    {
        Pendiente,
        Aprobada,
        Rechazada,
        Cancelada
    }

    public enum PrioridadSolicitud
    {
        Baja,
        Media,
        Alta,
        Urgente
    }
}
