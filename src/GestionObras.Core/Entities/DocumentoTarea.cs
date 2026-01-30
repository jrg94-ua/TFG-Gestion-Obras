using System;

namespace GestionObras.Core.Entities
{
    /// <summary>
    /// Representa un documento adjunto a una tarea (informes, planos, certificaciones, etc.)
    /// </summary>
    public class DocumentoTarea
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Nombre del archivo
        /// </summary>
        public string NombreArchivo { get; set; } = string.Empty;
        
        /// <summary>
        /// Ruta donde se almacena el archivo
        /// </summary>
        public string RutaArchivo { get; set; } = string.Empty;
        
        /// <summary>
        /// Tipo de documento (Informe, Certificado, Plano, Foto, Otro)
        /// </summary>
        public TipoDocumento TipoDocumento { get; set; }
        
        /// <summary>
        /// Tamaño del archivo en bytes
        /// </summary>
        public long TamañoBytes { get; set; }
        
        /// <summary>
        /// Tipo MIME del archivo
        /// </summary>
        public string TipoMime { get; set; } = string.Empty;
        
        /// <summary>
        /// Fecha en que se subió el documento
        /// </summary>
        public DateTime FechaSubida { get; set; }
        
        /// <summary>
        /// Usuario que subió el documento
        /// </summary>
        public string UsuarioSubidaId { get; set; } = string.Empty;
        public UsuarioObra? UsuarioSubida { get; set; }
        
        /// <summary>
        /// Descripción o notas sobre el documento
        /// </summary>
        public string? Descripcion { get; set; }
        
        // Relación con la tarea
        public int TareaId { get; set; }
        public Tarea Tarea { get; set; } = null!;
    }
    
    public enum TipoDocumento
    {
        Informe,
        Certificado,
        Plano,
        Fotografia,
        ParteTrabajo,
        Presupuesto,
        Factura,
        Otro
    }
}
