# Integración Normativa

## Introducción

Este documento detalla cómo el sistema integra la normativa técnica española de edificación de forma **automatizada**, garantizando el cumplimiento legal en todos los proyectos.

---

## 1. Marco Normativo Español

### 1.1 Jerarquía Normativa en Construcción

```
┌─────────────────────────────────────────────────────────┐
│        NIVEL ESTATAL (Aplicación en toda España)        │
│  • Código Técnico de la Edificación (CTE)              │
│  • Boletín Oficial del Estado (BOE)                    │
│  Fuente: Ministerio de Transportes y Vivienda          │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│       NIVEL AUTONÓMICO (Comunidad Valenciana)           │
│  • LOTUP (Ley de Ordenación del Territorio)            │
│  • DOCV (Diari Oficial de la Comunitat Valenciana)     │
│  Fuente: Generalitat Valenciana                        │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│           NIVEL LOCAL (Municipal)                       │
│  • Plan General de Ordenación Urbana (PGOU)            │
│  • Ordenanzas Municipales                              │
│  Fuente: Ayuntamientos (Sede Electrónica)              │
└─────────────────────────────────────────────────────────┘
```

---

## 2. Código Técnico de la Edificación (CTE)

### 2.1 Estructura del CTE

El CTE se organiza en **Documentos Básicos (DB)** que regulan aspectos técnicos específicos:

| Documento Básico | Código | Ámbito | Ejemplo de Requisito |
|------------------|--------|--------|----------------------|
| Seguridad Estructural | DB-SE | Resistencia y estabilidad | Capacidad portante mínima |
| Seguridad en caso de Incendio | DB-SI | Protección contra fuego | Resistencia RF-120 en muros |
| Seguridad de Utilización | DB-SUA | Prevención de accidentes | Altura de barandillas |
| Salubridad | DB-HS | Higiene y salud | Impermeabilización |
| Protección frente al Ruido | DB-HR | Aislamiento acústico | Nivel máximo de decibelios |
| **Ahorro de Energía** | **DB-HE** | **Eficiencia energética** | **Transmitancia térmica máxima** |

### 2.2 Integración Técnica con el CTE

#### 2.2.1 Fuente de Datos: Catálogo de Elementos Constructivos (CEC)

El Ministerio publica el **Catálogo de Elementos Constructivos** en formato **XML**, accesible en:

```
https://www.codigotecnico.org/DocumentosCTE/Catalogo.html
```

**Estructura del XML**:
```xml
<CatalogoElementos>
  <Material>
    <Codigo>XPS_80</Codigo>
    <Nombre>Panel aislante XPS 80mm</Nombre>
    <Propiedades>
      <TransmitanciaTermica>0.35</TransmitanciaTermica> <!-- W/m²K -->
      <ClasificacionFuego>E</ClasificacionFuego>
      <Densidad>35</Densidad> <!-- kg/m³ -->
    </Propiedades>
    <DocumentosBasicos>
      <DB>HE</DB>
      <DB>SI</DB>
    </DocumentosBasicos>
  </Material>
</CatalogoElementos>
```

#### 2.2.2 Implementación del Servicio CTE

```csharp
public class ServicioCTE : IServicioCatalogoMateriales
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    
    public async Task<Material> ObtenerMaterialDelCatalogo(string codigo)
    {
        // 1. Descargar XML del Ministerio
        var xmlData = await _cache.GetOrCreateAsync(
            "catalogo_cte",
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30);
                return await _httpClient.GetStringAsync(
                    "https://www.codigotecnico.org/CEC/Catalogo.xml"
                );
            });
        
        // 2. Parsear XML y buscar material
        var doc = XDocument.Parse(xmlData);
        var materialNode = doc.Descendants("Material")
            .FirstOrDefault(m => m.Element("Codigo")?.Value == codigo);
        
        if (materialNode == null)
            throw new MaterialNoEncontradoException(codigo);
        
        // 3. Mapear a entidad de dominio
        return new Material
        {
            Codigo = materialNode.Element("Codigo").Value,
            Nombre = materialNode.Element("Nombre").Value,
            TransmitanciaTermica = decimal.Parse(
                materialNode.Element("Propiedades")
                    .Element("TransmitanciaTermica").Value
            ),
            ClasificacionFuego = materialNode.Element("Propiedades")
                .Element("ClasificacionFuego").Value
        };
    }
    
    public bool ValidarCumplimientoDB_HE(
        Material material, 
        ZonaClimatica zona)
    {
        // Tabla de límites según zona climática (DB-HE)
        var limites = new Dictionary<ZonaClimatica, decimal>
        {
            { ZonaClimatica.A3, 0.50m },
            { ZonaClimatica.B3, 0.38m }, // Valencia
            { ZonaClimatica.C3, 0.29m },
            { ZonaClimatica.D3, 0.27m },
            { ZonaClimatica.E1, 0.25m }
        };
        
        var limiteMaximo = limites[zona];
        return material.TransmitanciaTermica <= limiteMaximo;
    }
}
```

---

## 3. Vigilancia del BOE (Boletín Oficial del Estado)

### 3.1 Estrategia de Monitorización

El sistema se suscribe al **feed RSS del BOE** para detectar cambios normativos en tiempo real.

#### 3.1.1 URL del Feed RSS
```
https://www.boe.es/rss/index.php?c=2200
```
*(Sección 2200: Código Técnico de la Edificación)*

#### 3.1.2 Estructura de la Respuesta RSS

```xml
<rss version="2.0">
  <channel>
    <title>BOE - Código Técnico de la Edificación</title>
    <item>
      <title>Orden VIV/984/2023 - Modificación DB-HE</title>
      <link>https://www.boe.es/diario_boe/txt.php?id=BOE-A-2023-12345</link>
      <pubDate>Mon, 15 Jan 2024 00:00:00 +0100</pubDate>
      <description>
        Modificación de transmitancia térmica en fachadas
      </description>
    </item>
  </channel>
</rss>
```

#### 3.1.3 Implementación del Servicio BOE

```csharp
public class ServicioBOE : IServicioNormativaEstatal
{
    private readonly HttpClient _httpClient;
    private readonly IProyectoRepository _proyectoRepo;
    private readonly INotificacionService _notificaciones;
    
    // Tarea en background que se ejecuta cada 24 horas
    public async Task MonitorizarCambiosNormativos()
    {
        // 1. Descargar feed RSS
        var rssXml = await _httpClient.GetStringAsync(
            "https://www.boe.es/rss/index.php?c=2200"
        );
        
        // 2. Parsear RSS
        var doc = XDocument.Parse(rssXml);
        var cambiosRecientes = doc.Descendants("item")
            .Select(item => new CambioNormativo
            {
                Titulo = item.Element("title").Value,
                Url = item.Element("link").Value,
                FechaPublicacion = DateTime.Parse(item.Element("pubDate").Value),
                Descripcion = item.Element("description").Value
            })
            .Where(c => c.FechaPublicacion > DateTime.Now.AddDays(-1))
            .ToList();
        
        // 3. Si hay cambios, notificar a proyectos afectados
        foreach (var cambio in cambiosRecientes)
        {
            if (AfectaAProyectosActivos(cambio))
            {
                await NotificarCambioNormativo(cambio);
            }
        }
    }
    
    private async Task NotificarCambioNormativo(CambioNormativo cambio)
    {
        var proyectosActivos = await _proyectoRepo.ObtenerProyectosEnCurso();
        
        foreach (var proyecto in proyectosActivos)
        {
            await _notificaciones.EnviarNotificacionPushAsync(
                proyecto.JefeDeObra.UserId,
                $"⚠️ CAMBIO NORMATIVO: {cambio.Titulo}",
                $"Se ha publicado una modificación del CTE que puede afectar " +
                $"al proyecto '{proyecto.Nombre}'. Revisa: {cambio.Url}"
            );
        }
    }
}
```

---

## 4. LOTUP (Ley de Ordenación del Territorio - C.V.)

### 4.1 Casos de Activación

La **LOTUP** solo se activa cuando se cumplen **dos condiciones**:
1. Ubicación: Comunidad Valenciana
2. Tipo de suelo: **Rústico** (no urbanizable)

### 4.2 Restricciones Principales de la LOTUP

| Parámetro | Requisito | Objetivo |
|-----------|-----------|----------|
| **Distancia mínima al núcleo urbano** | 200 metros | Evitar urbanización dispersa |
| **Ocupación máxima** | 2% de la parcela | Proteger suelo agrícola |
| **Altura máxima** | 7 metros (2 plantas) | Integración paisajística |
| **Uso permitido** | Vivienda vinculada a explotación | Prevenir especulación |

### 4.3 Implementación del Servicio LOTUP

```csharp
public class ServicioLOTUP : IServicioNormativaAutonomica
{
    public ResultadoValidacion ValidarProyectoRustico(Proyecto proyecto)
    {
        var errores = new List<string>();
        
        // 1. Validar distancia al núcleo urbano
        var distancia = CalcularDistanciaNucleoUrbano(proyecto.Ubicacion);
        if (distancia < 200)
        {
            errores.Add($"Distancia al núcleo urbano: {distancia}m. Mínimo: 200m");
        }
        
        // 2. Validar ocupación de parcela
        var ocupacion = (proyecto.SuperficieConstruida / proyecto.SuperficieParcela) * 100;
        if (ocupacion > 2)
        {
            errores.Add($"Ocupación de parcela: {ocupacion:F2}%. Máximo: 2%");
        }
        
        // 3. Validar altura
        if (proyecto.AlturaMaxima > 7)
        {
            errores.Add($"Altura máxima: {proyecto.AlturaMaxima}m. Máximo: 7m");
        }
        
        return new ResultadoValidacion
        {
            EsValido = !errores.Any(),
            Errores = errores
        };
    }
}
```

---

## 5. Inteligencia Artificial para PGOU Municipales

### 5.1 Desafío Técnico

Los **Planes Generales** no están estandarizados:
- Cada ayuntamiento tiene su propia web
- Formatos: PDF, Word, páginas HTML
- Estructuras documentales diferentes

### 5.2 Arquitectura de IA Híbrida

```
┌─────────────────────────────────────────────────────────┐
│                 FASE 1: DESCUBRIMIENTO                  │
│  ┌────────────────────────────────────────────────┐    │
│  │  API Externa (OpenAI/Tavily)                   │    │
│  │  • Busca en Google/Bing                        │    │
│  │  • Identifica sede electrónica del municipio   │    │
│  │  • Localiza URL del PGOU                       │    │
│  └────────────────────────────────────────────────┘    │
└───────────────────────┬─────────────────────────────────┘
                        │
                        ▼ URL del PGOU
┌─────────────────────────────────────────────────────────┐
│              FASE 2: PROCESAMIENTO LOCAL                │
│  ┌────────────────────────────────────────────────┐    │
│  │  Llama 3 (Open Source via Ollama)             │    │
│  │  • Descarga PDF del PGOU                      │    │
│  │  • Extrae texto (OCR si necesario)            │    │
│  │  • Aplica RAG (Retrieval-Augmented Generation)│    │
│  │  • Extrae parámetros urbanísticos             │    │
│  └────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
                        │
                        ▼
              Parámetros Urbanísticos
        (Altura, retranqueos, edificabilidad)
```

### 5.3 Ventajas de la Arquitectura Híbrida

| Aspecto | API Externa (Tavily) | Modelo Local (Llama 3) |
|---------|----------------------|------------------------|
| **Coste** | Por consulta (~0.02€) | Gratis tras instalación |
| **Uso** | Solo descubrimiento | Procesamiento intensivo |
| **Privacidad** | Datos enviados a terceros | Datos quedan en local |
| **Velocidad** | Rápido (< 5s) | Medio (10-30s según PDF) |

**Resultado**: **Coste total < 0.10€ por proyecto** (vs. 5-10€ si todo fuera con API)

### 5.4 Implementación del Servicio IA

```csharp
public class ServicioIAUrbanistica : IServicioInteligenciaLocal
{
    private readonly ITavilySearchClient _tavily;
    private readonly IOllamaClient _ollama;
    
    // FASE 1: Descubrimiento con API externa
    public async Task<string> LocalizarPGOU(string municipio, string provincia)
    {
        var query = $@"
            Plan General Ordenación Urbana {municipio} {provincia} 
            sede electrónica ayuntamiento oficial PDF
        ";
        
        var resultados = await _tavily.SearchAsync(new SearchRequest
        {
            Query = query,
            MaxResults = 5,
            IncludeDomains = new[] { ".gob.es", ".es" } // Solo dominios oficiales
        });
        
        // Filtrar por dominios gubernamentales
        var urlOficial = resultados
            .Where(r => r.Url.Contains(".gob.es") || r.Url.Contains("ayuntamiento"))
            .FirstOrDefault();
        
        if (urlOficial == null)
            throw new PGOUNoEncontradoException(municipio);
        
        return urlOficial.Url;
    }
    
    // FASE 2: Procesamiento local con RAG
    public async Task<ParametrosUrbanisticos> ExtraerParametros(
        string pdfUrl, 
        TipoZona zona)
    {
        // 1. Descargar PDF
        var pdfBytes = await DescargarPDF(pdfUrl);
        
        // 2. Extraer texto (con OCR si es escaneo)
        var textoPGOU = ExtraerTextoPDF(pdfBytes);
        
        // 3. Crear chunks para RAG (contexto limitado de Llama 3)
        var chunks = DividirEnChunks(textoPGOU, maxTokens: 4000);
        
        // 4. Buscar chunk relevante con búsqueda semántica
        var chunkRelevante = BuscarChunkConParametros(chunks, zona);
        
        // 5. Prompt estructurado para Llama 3
        var prompt = $@"
Eres un experto urbanista español. Del siguiente fragmento del Plan General 
de Ordenación Urbana, extrae ÚNICAMENTE los siguientes parámetros numéricos:

1. Altura máxima edificación (metros)
2. Retranqueo mínimo a linderos (metros)
3. Ocupación máxima de parcela (porcentaje)
4. Edificabilidad (m²/m²)
5. Restricciones estéticas (materiales, colores)

Fragmento del PGOU:
{chunkRelevante}

Responde SOLO en formato JSON:
{{
  ""alturaMaxima"": número,
  ""retranqueoMinimo"": número,
  ""ocupacionMaxima"": número,
  ""edificabilidad"": número,
  ""restriccionesEsteticas"": ""texto""
}}
";
        
        // 6. Generar con Llama 3
        var respuesta = await _ollama.GenerateAsync(
            model: "llama3",
            prompt: prompt,
            options: new GenerateOptions
            {
                Temperature = 0.1, // Baja temperatura para respuestas precisas
                TopP = 0.9
            });
        
        // 7. Parsear JSON y mapear a entidad
        var json = JObject.Parse(respuesta.Response);
        return new ParametrosUrbanisticos
        {
            AlturaMaxima = json["alturaMaxima"].Value<decimal>(),
            RetranqueoMinimo = json["retranqueoMinimo"].Value<decimal>(),
            OcupacionMaxima = json["ocupacionMaxima"].Value<decimal>(),
            Edificabilidad = json["edificabilidad"].Value<decimal>(),
            RestriccionesEsteticas = json["restriccionesEsteticas"].Value<string>()
        };
    }
    
    // Búsqueda semántica con embeddings
    private string BuscarChunkConParametros(List<string> chunks, TipoZona zona)
    {
        // Palabras clave según tipo de zona
        var keywords = zona switch
        {
            TipoZona.Residencial => new[] { "altura", "vivienda", "edificabilidad" },
            TipoZona.Industrial => new[] { "nave", "almacén", "ocupación" },
            TipoZona.Terciario => new[] { "comercial", "oficina", "aparcamiento" },
            _ => new[] { "edificación", "construcción", "parámetros" }
        };
        
        // Calcular relevancia de cada chunk
        var chunksPuntuados = chunks.Select(chunk => new
        {
            Chunk = chunk,
            Puntuacion = keywords.Sum(kw => 
                chunk.ToLower().Split(' ').Count(word => word.Contains(kw))
            )
        });
        
        // Devolver chunk con mayor puntuación
        return chunksPuntuados
            .OrderByDescending(c => c.Puntuacion)
            .First()
            .Chunk;
    }
}
```

---

## 6. Flujo Completo de Validación Normativa

### Diagrama de Secuencia

```
Usuario (Jefe Obra)
  │
  ├─► Crear Proyecto
  │     ├─ Ubicación: Valencia
  │     ├─ Suelo: Rústico
  │     └─ Tipo: Vivienda unifamiliar
  │
  ▼
Sistema
  │
  ├─► [1] Activar Servicio BOE/CTE
  │     ├─ Descargar CTE vigente
  │     └─ Almacenar en Carpeta Legal
  │
  ├─► [2] Activar Servicio LOTUP (por ser suelo rústico)
  │     ├─ Validar distancia > 200m
  │     ├─ Validar ocupación < 2%
  │     └─ Generar warnings si no cumple
  │
  ├─► [3] Activar Servicio IA Local
  │     ├─ 3.1. Tavily: Buscar PGOU de Valencia
  │     │       └─ URL: https://www.valencia.es/pgou.pdf
  │     │
  │     ├─ 3.2. Llama 3: Descargar y procesar PDF
  │     │       └─ Extraer: Altura 7m, Retranqueo 3m
  │     │
  │     └─ 3.3. Almacenar parámetros en BD
  │
  └─► [4] Generar Carpeta Legal del Proyecto
        ├─ CTE (DB-SE, DB-HE, DB-SI)
        ├─ LOTUP (Artículos aplicables)
        ├─ PGOU Valencia (PDF original + parámetros extraídos)
        └─ Fecha de consulta (trazabilidad)
```

---

## 7. Casos de Uso Avanzados

### Caso 1: Actualización Retroactiva de Normativa

**Escenario**: El BOE publica una modificación del DB-HE que reduce la transmitancia térmica máxima de 0.38 a 0.35 W/m²K en zona B3.

**Acción del Sistema**:
1. Servicio BOE detecta el cambio en el feed RSS
2. Identifica proyectos activos en zona B3 (Valencia)
3. Verifica si los materiales aprobados siguen cumpliendo
4. Si material XPS_80 (0.35 W/m²K) **justo cumple**: ✅ Proyecto sigue siendo válido
5. Si material Lana_Vidrio (0.40 W/m²K) **NO cumple**: ❌ Notifica a Oficina Técnica

**Resultado**: La PYME evita sanciones de 6.000-60.000€ por incumplimiento normativo

---

### Caso 2: Comparación Multi-Municipal

**Escenario**: La PYME quiere expandirse a tres municipios diferentes de Valencia.

**Acción del Sistema**:
1. IA procesa PGOU de: Paterna, Torrent y Mislata
2. Genera tabla comparativa:

| Parámetro | Paterna | Torrent | Mislata |
|-----------|---------|---------|---------|
| Altura máxima | 12m | 10m | 15m |
| Retranqueo | 3m | 5m | 3m |
| Ocupación | 50% | 40% | 60% |
| Estética | Ladrillo cara vista | Sin restricciones | Colores neutros |

**Resultado**: El gerente identifica que **Mislata** es el municipio más favorable para el proyecto

---

## 8. Métricas de Éxito

| Indicador | Objetivo | Método de Medición |
|-----------|----------|-------------------|
| **Actualidad normativa** | < 24 horas de desfase con BOE | Timestamp última sincronización |
| **Precisión IA PGOU** | > 85% de parámetros correctos | Validación manual de 20 municipios |
| **Cobertura municipal** | > 90% de municipios > 5.000 hab. | Test de localización PGOU |
| **Tiempo de procesamiento** | < 2 min por proyecto | Log de tiempos de ejecución |
| **Ahorro en sanciones** | 0 sanciones por normativa obsoleta | Auditorías anuales |

---

**Documento elaborado**: Enero 2026  
**Versión**: 1.0  
**Autor**: Jorge Ros Gómez
