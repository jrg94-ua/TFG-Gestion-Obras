using System.Threading.Tasks;
using GestionObras.Core.Entities;

namespace GestionObras.Core.Interfaces
{
    /// <summary>
    /// Servicio de inteligencia normativa estatal (BOE/CTE)
    /// Requisitos funcionales: RF-02, RF-03
    /// </summary>
    public interface IServicioNormativaEstatal
    {
        /// <summary>
        /// Monitoriza cambios en el BOE relacionados con el CTE
        /// </summary>
        Task MonitorizarCambiosNormativosAsync();
        
        /// <summary>
        /// Obtiene el CTE vigente para un tipo de obra
        /// </summary>
        Task<string> ObtenerCTEVigenteAsync();
    }
    
    /// <summary>
    /// Servicio del Catálogo de Elementos Constructivos (CTE)
    /// Requisito funcional: RF-03
    /// </summary>
    public interface IServicioCatalogoMateriales
    {
        /// <summary>
        /// Obtiene las propiedades técnicas de un material desde el catálogo XML del CTE
        /// </summary>
        Task<Material?> ObtenerMaterialDelCatalogoAsync(string codigo);
        
        /// <summary>
        /// Valida si un material cumple con el DB-HE (Ahorro de Energía)
        /// </summary>
        bool ValidarCumplimientoDB_HE(Material material, ZonaClimatica zona);
    }
    
    /// <summary>
    /// Servicio de normativa autonómica (LOTUP para Comunidad Valenciana)
    /// Requisito funcional: RF-04
    /// </summary>
    public interface IServicioNormativaAutonomica
    {
        /// <summary>
        /// Valida un proyecto en suelo rústico según la LOTUP
        /// </summary>
        Task<ResultadoValidacion> ValidarProyectoRusticoAsync(Proyecto proyecto);
    }
    
    /// <summary>
    /// Servicio de IA para extracción de parámetros urbanísticos de PGOU
    /// Requisitos funcionales: RF-04, RF-05
    /// </summary>
    public interface IServicioInteligenciaLocal
    {
        /// <summary>
        /// Localiza el PGOU de un municipio usando búsqueda con IA (OpenAI/Tavily)
        /// </summary>
        Task<string> LocalizarPGOUAsync(string municipio, string provincia);
        
        /// <summary>
        /// Extrae parámetros urbanísticos del PDF del PGOU usando Llama 3 + RAG
        /// </summary>
        Task<ParametrosUrbanisticos> ExtraerParametrosAsync(string pdfUrl);
    }
    
    /// <summary>
    /// Resultado de validación normativa
    /// </summary>
    public class ResultadoValidacion
    {
        public bool EsValido { get; set; }
        public List<string> Errores { get; set; } = new();
        public List<string> Advertencias { get; set; } = new();
    }
    
    /// <summary>
    /// Parámetros urbanísticos extraídos del PGOU
    /// </summary>
    public class ParametrosUrbanisticos
    {
        public decimal AlturaMaxima { get; set; } // metros
        public decimal RetranqueoMinimo { get; set; } // metros
        public decimal OcupacionMaxima { get; set; } // porcentaje
        public decimal Edificabilidad { get; set; } // m²/m²
        public string RestriccionesEsteticas { get; set; } = string.Empty;
    }
}
