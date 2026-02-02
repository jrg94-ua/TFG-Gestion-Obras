using System;

namespace GestionObras.Core.Entities
{
    /// <summary>
    /// Representa una factura vinculada a un proyecto o tarea específica
    /// </summary>
    public class Factura
    {
        public int Id { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public decimal Importe { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public DateTime? FechaPago { get; set; }
        public EstadoFactura Estado { get; set; }
        public string Concepto { get; set; } = string.Empty;
        
        // Propiedades adicionales para gestión
        public decimal BaseImponible { get; set; }
        public decimal PorcentajeIVA { get; set; } = 21;
        public decimal IVA { get; set; }
        public decimal ImporteTotal { get; set; }
        public string? NombreProyecto { get; set; }
        public string? MetodoPago { get; set; }
        public string? Observaciones { get; set; }
        
        // Relaciones
        public int? ProyectoId { get; set; }
        public Proyecto? Proyecto { get; set; }
        public int? TareaId { get; set; }
        public Tarea? Tarea { get; set; }
        public int ProveedorId { get; set; }
        public Proveedor Proveedor { get; set; } = null!;
    }
    
    public enum EstadoFactura
    {
        Pendiente,
        Pagada,
        Vencida,
        Cancelada
    }
    
    /// <summary>
    /// Representa el presupuesto de un proyecto
    /// </summary>
    public class Presupuesto
    {
        public int Id { get; set; }
        public int ProyectoId { get; set; }
        public Proyecto Proyecto { get; set; } = null!;
        public decimal Total { get; set; }
        public DateTime FechaElaboracion { get; set; }
        public string Observaciones { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Carpeta legal del proyecto con toda la normativa aplicable (requisito funcional RF-20)
    /// </summary>
    public class CarpetaLegal
    {
        public int Id { get; set; }
        public int ProyectoId { get; set; }
        public Proyecto Proyecto { get; set; } = null!;
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimaActualizacion { get; set; }
        
        // Documentos normativos almacenados
        public string? DocumentoCTE { get; set; } // URL o ruta al PDF del CTE
        public string? DocumentoLOTUP { get; set; } // URL o ruta al PDF de la LOTUP
        public string? DocumentoPGOU { get; set; } // URL o ruta al PDF del PGOU municipal
        public string? ParametrosUrbanisticosJson { get; set; } // JSON con parámetros extraídos por IA
    }
}
