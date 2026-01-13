using System;

namespace GestionObras.Core.Entities
{
    /// <summary>
    /// Representa un material de construcción del catálogo CTE
    /// </summary>
    public class Material
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        
        // Propiedades técnicas del CTE
        public decimal? TransmitanciaTermica { get; set; } // W/m²K
        public string? ClasificacionFuego { get; set; } // A1, A2, B, C, D, E, F
        public decimal? Densidad { get; set; } // kg/m³
        public decimal? ResistenciaCompresion { get; set; } // MPa
        
        // Gestión de inventario
        public decimal PrecioUnitario { get; set; }
        public string UnidadMedida { get; set; } = string.Empty; // m², m³, ud, kg
        public int StockDisponible { get; set; }
        
        // Proveedor
        public int? ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }
        
        /// <summary>
        /// Valida si el material cumple con el DB-HE del CTE para una zona climática
        /// </summary>
        public bool ValidarCumplimientoDB_HE(ZonaClimatica zona)
        {
            if (!TransmitanciaTermica.HasValue)
                return false;
            
            // Límites según DB-HE (Ahorro de Energía)
            var limites = zona switch
            {
                ZonaClimatica.A3 => 0.50m,
                ZonaClimatica.B3 => 0.38m, // Valencia
                ZonaClimatica.C3 => 0.29m,
                ZonaClimatica.D3 => 0.27m,
                ZonaClimatica.E1 => 0.25m,
                _ => 0.50m
            };
            
            return TransmitanciaTermica.Value <= limites;
        }
    }
    
    /// <summary>
    /// Representa un proveedor de materiales
    /// </summary>
    public class Proveedor
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string CIF { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
