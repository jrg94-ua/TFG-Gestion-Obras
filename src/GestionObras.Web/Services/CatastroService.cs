using System.Text.Json;
using System.Xml.Linq;

namespace GestionObras.Web.Services
{
    public interface ICatastroService
    {
        Task<DatosCatastrales?> ObtenerDatosPorCoordenadas(double latitud, double longitud);
    }

    public class CatastroService : ICatastroService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CatastroService> _logger;

        public CatastroService(HttpClient httpClient, ILogger<CatastroService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<DatosCatastrales?> ObtenerDatosPorCoordenadas(double latitud, double longitud)
        {
            try
            {
                // El servicio REST tiene problemas, usar el antiguo pero estable
                var url = "http://ovc.catastro.meh.es/ovcservweb/OVCSWLocalizacionRC/OVCCoordenadas.asmx/Consulta_CPMRC";
                
                // Formato con parámetros en query string
                var queryString = $"?SRS=EPSG:4326&Coordenada_X={longitud.ToString(System.Globalization.CultureInfo.InvariantCulture)}&Coordenada_Y={latitud.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
                
                var fullUrl = url + queryString;
                
                _logger.LogInformation($"Consultando Catastro (ASMX): Lat={latitud}, Lng={longitud}");
                _logger.LogInformation($"URL: {fullUrl}");

                var response = await _httpClient.GetAsync(fullUrl);
                
                var xmlContent = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error HTTP: {response.StatusCode}");
                    _logger.LogError($"Respuesta: {xmlContent.Substring(0, Math.Min(500, xmlContent.Length))}");
                    return null;
                }

                _logger.LogInformation($"Respuesta OK, tamaño: {xmlContent.Length} caracteres");
                
                return ParsearRespuestaCatastro(xmlContent, latitud, longitud);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos del catastro");
                return null;
            }
        }

        private DatosCatastrales? ParsearRespuestaCatastro(string xmlContent, double latitud, double longitud)
        {
            try
            {
                var doc = XDocument.Parse(xmlContent);
                
                // Verificar errores en el bloque <control>
                var control = doc.Descendants("control").FirstOrDefault();
                if (control != null)
                {
                    var codigoError = control.Element("cuerr")?.Value;
                    var descripcionError = control.Element("des")?.Value;
                    
                    if (!string.IsNullOrEmpty(codigoError) && codigoError != "0")
                    {
                        _logger.LogWarning($"Error del Catastro - Código: {codigoError}, Descripción: {descripcionError}");
                        
                        // Error 15: Error al buscar coordenadas
                        // Error 16: No hay referencia catastral
                        if (codigoError == "15" || codigoError == "16")
                        {
                            return null;
                        }
                    }
                }
                
                // Extraer coordenadas y referencia catastral
                var coordenadas = doc.Descendants("coordenadas").FirstOrDefault();
                if (coordenadas == null)
                {
                    _logger.LogWarning("No se encontró elemento 'coordenadas' en la respuesta");
                    return null;
                }

                var coord = coordenadas.Descendants("coord").FirstOrDefault();
                if (coord == null)
                {
                    _logger.LogWarning("No se encontró elemento 'coord' en la respuesta");
                    return null;
                }

                // Extraer PC1 y PC2 para formar la Referencia Catastral
                var pc = coord.Element("pc");
                var pc1 = pc?.Element("pc1")?.Value ?? "";
                var pc2 = pc?.Element("pc2")?.Value ?? "";
                var referenciaCatastral = $"{pc1}{pc2}";
                
                // Dirección literal completa
                var ldt = coord.Element("ldt")?.Value ?? "";

                // Extraer provincia y municipio de la estructura
                var provincia = "";
                var municipio = "";
                
                var locs = coord.Descendants("locs").FirstOrDefault();
                if (locs != null)
                {
                    var lourb = locs.Descendants("lourb").FirstOrDefault();
                    if (lourb != null)
                    {
                        municipio = lourb.Element("dm")?.Value ?? "";
                        var loine = lourb.Element("loine");
                        if (loine != null)
                        {
                            provincia = loine.Element("cm")?.Value ?? "";
                        }
                    }
                }

                // Si no hay datos estructurados, intentar extraer de ldt
                if (string.IsNullOrEmpty(provincia))
                    provincia = ExtraerProvincia(ldt);
                
                if (string.IsNullOrEmpty(municipio))
                    municipio = ExtraerMunicipio(ldt);

                var resultado = new DatosCatastrales
                {
                    ReferenciaCalastral = referenciaCatastral,
                    Direccion = ldt,
                    Provincia = provincia,
                    Municipio = municipio,
                    Latitud = latitud,
                    Longitud = longitud
                };

                _logger.LogInformation($"Datos obtenidos: RC={referenciaCatastral}, Dirección={ldt}, Provincia={provincia}, Municipio={municipio}");
                
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al parsear XML del catastro");
                return null;
            }
        }

        private string ExtraerProvincia(string direccion)
        {
            // Extraer provincia de la dirección completa
            // Formato típico: "CL NOMBRE 123 28001 MADRID (MADRID)"
            var partes = direccion.Split('(', ')');
            if (partes.Length >= 2)
            {
                return partes[1].Trim();
            }
            return "";
        }

        private string ExtraerMunicipio(string direccion)
        {
            // Extraer municipio de la dirección
            var partes = direccion.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length >= 2)
            {
                // Buscar el código postal (5 dígitos) y tomar la palabra siguiente
                for (int i = 0; i < partes.Length - 1; i++)
                {
                    if (partes[i].Length == 5 && int.TryParse(partes[i], out _))
                    {
                        return partes[i + 1];
                    }
                }
            }
            return "";
        }
    }

    public class DatosCatastrales
    {
        public string ReferenciaCalastral { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Provincia { get; set; } = string.Empty;
        public string Municipio { get; set; } = string.Empty;
        public double Latitud { get; set; }
        public double Longitud { get; set; }
    }
}
