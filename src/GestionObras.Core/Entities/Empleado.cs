using System;
using System.Collections.Generic;
using System.Linq;

namespace GestionObras.Core.Entities
{
    /// <summary>
    /// Representa un empleado de la constructora
    /// </summary>
    public class Empleado
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string DNI { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public CategoriaLaboral Categoria { get; set; }
        
        // Relación con usuario del sistema
        public string? UsuarioId { get; set; }
        public UsuarioObra? Usuario { get; set; }
        
        // Relaciones
        public List<CursoPRL> CursosPRL { get; set; } = new();
        public List<Fichaje> Fichajes { get; set; } = new();
        public List<Proyecto> ProyectosAsignados { get; set; } = new();
        
        /// <summary>
        /// Verifica si el empleado tiene formación PRL vigente (requisito funcional RF-13)
        /// </summary>
        public bool TienePRLVigente()
        {
            return CursosPRL.Any(curso => curso.EstaVigente());
        }
    }
    
    public enum CategoriaLaboral
    {
        Peon,
        OficialTercera,
        OficialSegunda,
        OficialPrimera,
        Encargado,
        JefeObra
    }
    
    /// <summary>
    /// Representa un curso de Prevención de Riesgos Laborales
    /// </summary>
    public class CursoPRL
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public Empleado Empleado { get; set; } = null!;
        public string NombreCurso { get; set; } = string.Empty;
        public TipoCursoPRL Tipo { get; set; }
        public DateTime FechaRealizacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public string EntidadCertificadora { get; set; } = string.Empty;
        public string NumeroCertificado { get; set; } = string.Empty;
        
        /// <summary>
        /// Verifica si el curso está vigente
        /// </summary>
        public bool EstaVigente()
        {
            return FechaExpiracion > DateTime.Now;
        }
    }
    
    public enum TipoCursoPRL
    {
        NivelBasico,
        TrabajoEnAltura,
        ManejoMaquinaria,
        EspaciosConfinados,
        RiesgoElectrico
    }
    
    /// <summary>
    /// Representa un fichaje de entrada/salida geolocalizado (requisito funcional RF-11)
    /// </summary>
    public class Fichaje
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public Empleado Empleado { get; set; } = null!;
        public int ProyectoId { get; set; }
        public Proyecto Proyecto { get; set; } = null!;
        public DateTime FechaHora { get; set; }
        public TipoFichaje Tipo { get; set; }
        
        // Geolocalización
        public double Latitud { get; set; }
        public double Longitud { get; set; }
    }
    
    public enum TipoFichaje
    {
        Entrada,
        Salida
    }
}
