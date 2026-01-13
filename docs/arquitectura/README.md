# Arquitectura del Sistema

## Visión General

El **Sistema de Gestión de Obras** se diseña siguiendo los principios de **Clean Architecture** y **Domain-Driven Design (DDD)**, garantizando separación de responsabilidades, mantenibilidad y escalabilidad.

---

## 1. Arquitectura de Alto Nivel

```
┌─────────────────────────────────────────────────────────────────┐
│                        CAPA DE PRESENTACIÓN                      │
│  ┌──────────────────┐          ┌──────────────────┐            │
│  │   Blazor Web     │          │   API REST       │            │
│  │   (Frontend)     │◄────────►│  (Controllers)   │            │
│  └──────────────────┘          └──────────────────┘            │
└──────────────────────────────────┬──────────────────────────────┘
                                   │
┌──────────────────────────────────▼──────────────────────────────┐
│                      CAPA DE APLICACIÓN                          │
│  ┌────────────────────────────────────────────────────────┐    │
│  │          Servicios de Aplicación (Use Cases)           │    │
│  │  • GestionProyectosService                             │    │
│  │  • GestionOperacionesService                           │    │
│  │  • GestionRRHHService                                  │    │
│  │  • GestionEconomicaService                             │    │
│  └────────────────────────────────────────────────────────┘    │
└──────────────────────────────────┬──────────────────────────────┘
                                   │
┌──────────────────────────────────▼──────────────────────────────┐
│                       CAPA DE DOMINIO                            │
│  ┌────────────────────────────────────────────────────────┐    │
│  │              Entidades de Negocio (Core)               │    │
│  │  • Proyecto, Tarea, Empleado, Material, Factura        │    │
│  │                                                         │    │
│  │              Lógica de Negocio (Domain Services)       │    │
│  │  • CalculadorROI                                       │    │
│  │  • ValidadorNormativa                                  │    │
│  │  • GestorBloqueos                                      │    │
│  └────────────────────────────────────────────────────────┘    │
└──────────────────────────────────┬──────────────────────────────┘
                                   │
┌──────────────────────────────────▼──────────────────────────────┐
│                    CAPA DE INFRAESTRUCTURA                       │
│  ┌───────────────┐  ┌────────────────┐  ┌──────────────────┐  │
│  │   SQL Server  │  │ Servicios      │  │   IA Services    │  │
│  │   (EF Core)   │  │ Externos       │  │   (Ollama/       │  │
│  │               │  │ • BOE (RSS)    │  │    OpenAI)       │  │
│  │               │  │ • CTE (XML)    │  │                  │  │
│  └───────────────┘  └────────────────┘  └──────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 2. Estructura de Capas

### 2.1 Capa de Presentación (GestionObras.Web / GestionObras.API)

**Responsabilidad**: Exponer funcionalidades al usuario final y clientes externos

**Componentes**:
- **Controllers REST**: Endpoints para operaciones CRUD
- **Blazor Components**: Interfaz de usuario interactiva
- **DTOs (Data Transfer Objects)**: Objetos para comunicación API

**Tecnologías**:
- ASP.NET Core 8.0 Web API
- Blazor Server/WebAssembly
- Bootstrap 5 para UI responsive

**Ejemplo de Endpoints**:
```
POST   /api/proyectos              # Crear nuevo proyecto
GET    /api/proyectos/{id}/kanban  # Obtener tablero Kanban
POST   /api/empleados/fichaje      # Registrar fichaje geolocalizado
GET    /api/dashboard/roi          # Obtener métricas de ROI
```

---

### 2.2 Capa de Aplicación (GestionObras.Application)

**Responsabilidad**: Orquestar casos de uso y coordinar lógica de negocio

**Componentes**:
- **Servicios de Aplicación**: Implementan casos de uso específicos
- **DTOs y Mappers**: Transformación entre entidades de dominio y DTOs
- **Validadores**: Reglas de validación de entrada

**Patrones Utilizados**:
- **CQRS (Command Query Responsibility Segregation)**: Separación de comandos y consultas
- **Mediator**: Coordinación de operaciones complejas

**Servicios Principales**:
```csharp
// Ejemplo de servicio de aplicación
public class GestionProyectosService
{
    // Caso de uso: Crear proyecto con validación normativa
    Task<ProyectoDto> CrearProyectoAsync(CrearProyectoCommand command);
    
    // Caso de uso: Actualizar carpeta legal del proyecto
    Task ActualizarCarpetaLegalAsync(int proyectoId);
}
```

---

### 2.3 Capa de Dominio (GestionObras.Core)

**Responsabilidad**: Contener la lógica de negocio pura, independiente de tecnología

**Componentes**:
- **Entidades**: Objetos de dominio con identidad propia
- **Value Objects**: Objetos sin identidad (ej: Dirección, CoordenadaGPS)
- **Domain Services**: Lógica que no pertenece a una entidad específica
- **Interfaces de Repositorio**: Contratos para acceso a datos

**Entidades Principales**:
```csharp
public class Proyecto
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public Ubicacion Ubicacion { get; set; }
    public TipoSuelo TipoSuelo { get; set; }
    public List<Tarea> Tareas { get; set; }
    public CarpetaLegal CarpetaLegal { get; set; }
    public Presupuesto Presupuesto { get; set; }
    
    // Lógica de negocio
    public bool PuedeIniciarTarea(Tarea tarea)
    {
        // Validar que empleados tengan PRL vigente
        // Validar que materiales estén disponibles
    }
}
```

**Domain Services**:
```csharp
public class CalculadorROI
{
    public decimal CalcularROIActual(Proyecto proyecto)
    {
        var costesReales = proyecto.ObtenerCostesReales();
        var presupuestoInicial = proyecto.Presupuesto.Total;
        var beneficioProyectado = presupuestoInicial - costesReales;
        return (beneficioProyectado / costesReales) * 100;
    }
}
```

---

### 2.4 Capa de Infraestructura (GestionObras.Infrastructure)

**Responsabilidad**: Implementar detalles técnicos de acceso a datos y servicios externos

**Componentes**:
- **Repositorios**: Implementación de interfaces de dominio con EF Core
- **DbContext**: Configuración de Entity Framework
- **Servicios Externos**: Integración con BOE, CTE, IA

**Subcapas**:

#### 2.4.1 Persistencia (Data)
```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Proyecto> Proyectos { get; set; }
    public DbSet<Empleado> Empleados { get; set; }
    public DbSet<Tarea> Tareas { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuración de relaciones, índices, etc.
    }
}
```

#### 2.4.2 Servicios de Inteligencia Normativa

**Servicio BOE (RSS)**:
```csharp
public class ServicioBOE : IServicioNormativaEstatal
{
    private readonly HttpClient _httpClient;
    
    public async Task<List<ActualizacionNormativa>> ObtenerCambiosRecientes()
    {
        var feed = await _httpClient.GetStringAsync("https://www.boe.es/rss/");
        // Parsear RSS y filtrar cambios relevantes del CTE
        return ParsearCambiosCTE(feed);
    }
}
```

**Servicio CTE (XML)**:
```csharp
public class ServicioCTE : IServicioCatalogoMateriales
{
    public async Task<Material> ObtenerPropiedadesMaterial(string codigoMaterial)
    {
        // Descargar XML del Catálogo de Elementos Constructivos
        var xmlData = await DescargarCatalogoXML();
        
        // Parsear y extraer propiedades técnicas
        return ExtraerMaterialDeXML(xmlData, codigoMaterial);
    }
    
    public bool ValidarCumplimientoDocumentoBasico(
        Material material, 
        DocumentoBasico db, 
        ZonaClimatica zona)
    {
        // Validar transmitancia térmica, resistencia al fuego, etc.
    }
}
```

**Servicio IA Local (RAG con Llama 3)**:
```csharp
public class ServicioIAUrbanistica : IServicioInteligenciaLocal
{
    private readonly IOllamaClient _ollamaClient;
    private readonly ITavilySearchClient _searchClient;
    
    // Capa 1: Descubrimiento con API externa
    public async Task<string> LocalizarPGOU(string municipio, string provincia)
    {
        var query = $"Plan General Ordenación Urbana {municipio} {provincia} sede electrónica";
        var resultados = await _searchClient.SearchAsync(query);
        return resultados.FirstOrDefault()?.Url;
    }
    
    // Capa 2: Procesamiento local con RAG
    public async Task<ParametrosUrbanisticos> ExtraerParametros(string pdfUrl)
    {
        // Descargar PDF
        var pdfBytes = await DescargarPDF(pdfUrl);
        
        // Extraer texto del PDF
        var texto = ExtraerTextoPDF(pdfBytes);
        
        // Crear contexto RAG para Llama 3
        var prompt = $@"
Del siguiente documento del PGOU, extrae:
- Altura máxima permitida
- Retranqueos obligatorios
- Ocupación máxima del suelo
- Restricciones estéticas

Documento:
{texto}
";
        
        var respuesta = await _ollamaClient.GenerateAsync("llama3", prompt);
        return ParsearRespuestaIA(respuesta);
    }
}
```

---

## 3. Diagrama de Dependencias

```
┌──────────────────────────────────────────────────────────────┐
│                         Presentación                          │
│                     (API + Blazor Web)                        │
└────────────────────────────┬─────────────────────────────────┘
                             │ depende de
                             ▼
┌──────────────────────────────────────────────────────────────┐
│                         Aplicación                            │
│                   (Servicios de Use Cases)                    │
└────────────────────────────┬─────────────────────────────────┘
                             │ depende de
                             ▼
┌──────────────────────────────────────────────────────────────┐
│                           Dominio                             │
│                    (Entidades + Lógica)                       │
│                   ◄─── NO DEPENDE DE NADIE                    │
└──────────────────────────────┬─────────────────────────────┬─┘
                               │ implementado por            │
                               ▼                             ▼
                    ┌──────────────────┐      ┌──────────────────┐
                    │  Infraestructura │      │   Servicios      │
                    │   (EF Core)      │      │   Externos       │
                    └──────────────────┘      └──────────────────┘
```

**Principio clave**: La capa de Dominio es el **centro** y **no depende de nadie**. Todas las demás capas dependen de ella.

---

## 4. Patrones de Diseño Aplicados

### 4.1 Repository Pattern
- **Objetivo**: Abstraer el acceso a datos del dominio
- **Implementación**: Interfaces en Dominio, implementación en Infraestructura

```csharp
// Dominio: Interfaz
public interface IProyectoRepository
{
    Task<Proyecto> ObtenerPorIdAsync(int id);
    Task<List<Proyecto>> ObtenerTodosAsync();
    Task AgregarAsync(Proyecto proyecto);
    Task ActualizarAsync(Proyecto proyecto);
}

// Infraestructura: Implementación
public class ProyectoRepository : IProyectoRepository
{
    private readonly ApplicationDbContext _context;
    
    public async Task<Proyecto> ObtenerPorIdAsync(int id)
    {
        return await _context.Proyectos
            .Include(p => p.Tareas)
            .Include(p => p.CarpetaLegal)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
```

### 4.2 Unit of Work Pattern
- **Objetivo**: Coordinar transacciones de múltiples repositorios
- **Beneficio**: Garantizar consistencia en operaciones complejas

```csharp
public interface IUnitOfWork : IDisposable
{
    IProyectoRepository Proyectos { get; }
    IEmpleadoRepository Empleados { get; }
    IFacturaRepository Facturas { get; }
    
    Task<int> SaveChangesAsync();
}
```

### 4.3 Strategy Pattern (Servicios de Normativa)
- **Objetivo**: Intercambiar algoritmos de validación según contexto
- **Aplicación**: Diferentes estrategias para Suelo Urbano vs Rústico

```csharp
public interface IEstrategiaValidacionSuelo
{
    Task<ResultadoValidacion> ValidarAsync(Proyecto proyecto);
}

public class ValidadorSueloUrbano : IEstrategiaValidacionSuelo
{
    public async Task<ResultadoValidacion> ValidarAsync(Proyecto proyecto)
    {
        // Solo valida CTE + PGOU
    }
}

public class ValidadorSueloRustico : IEstrategiaValidacionSuelo
{
    public async Task<ResultadoValidacion> ValidarAsync(Proyecto proyecto)
    {
        // Valida CTE + LOTUP + PGOU (más restrictivo)
    }
}
```

### 4.4 Specification Pattern (Consultas Complejas)
- **Objetivo**: Encapsular criterios de búsqueda reutilizables
- **Aplicación**: Filtros complejos para empleados, tareas, etc.

```csharp
public class EmpleadoConPRLVigenteSpec : Specification<Empleado>
{
    public override Expression<Func<Empleado, bool>> ToExpression()
    {
        return empleado => empleado.CursosPRL
            .Any(curso => curso.FechaExpiracion > DateTime.Now);
    }
}
```

---

## 5. Seguridad y Autenticación

### 5.1 Arquitectura de Seguridad

```
Usuario
  │
  ├─► [Login] ──► ASP.NET Core Identity ──► JWT Token
  │
  └─► [Request] ──► Middleware Autenticación
                       │
                       ├─► Valida Token
                       ├─► Carga Claims (Rol, ID Usuario)
                       └─► Autorización (RBAC)
                             │
                             ├─► [Admin] ──► Acceso Total
                             ├─► [JefeObra] ──► Sin acceso ROI/Facturas
                             ├─► [OficinaTecnica] ──► Solo config técnica
                             └─► [Operario] ──► Solo fichaje/tareas
```

### 5.2 Configuración de Roles

```csharp
public static class Roles
{
    public const string Administrador = "Administrador";
    public const string JefeObra = "JefeObra";
    public const string OficinaTecnica = "OficinaTecnica";
    public const string Operario = "Operario";
}

// Ejemplo de protección de endpoint
[Authorize(Roles = Roles.Administrador)]
public class DashboardROIController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardROIDto>> ObtenerROI()
    {
        // Solo gerentes pueden ver este dashboard
    }
}
```

---

## 6. Escalabilidad y Rendimiento

### 6.1 Estrategias de Caché

```csharp
public class ServicioCTEConCache : IServicioCatalogoMateriales
{
    private readonly IMemoryCache _cache;
    private readonly ServicioCTE _servicioBase;
    
    public async Task<Material> ObtenerPropiedadesMaterial(string codigo)
    {
        return await _cache.GetOrCreateAsync(
            $"material_{codigo}",
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30);
                return await _servicioBase.ObtenerPropiedadesMaterial(codigo);
            });
    }
}
```

### 6.2 Optimización de Consultas (EF Core)

```csharp
// ❌ MAL: N+1 queries
var proyectos = await _context.Proyectos.ToListAsync();
foreach (var proyecto in proyectos)
{
    var tareas = proyecto.Tareas; // Lazy loading ❌
}

// ✅ BIEN: Eager loading
var proyectos = await _context.Proyectos
    .Include(p => p.Tareas)
    .Include(p => p.CarpetaLegal)
    .AsSplitQuery() // Evita cartesian explosion
    .ToListAsync();
```

---

## 7. Despliegue en Cloud

### Arquitectura en Azure

```
Internet
   │
   └─► Azure Front Door (CDN + WAF)
         │
         └─► Azure App Service (Web API + Blazor)
               │
               ├─► Azure SQL Database (Principal)
               │     │
               │     └─► Geo-replication (Secundaria)
               │
               ├─► Azure Blob Storage (Documentos PDF, fotos)
               │
               ├─► Azure Cognitive Services (IA alternativa a Ollama)
               │
               └─► Azure Key Vault (Secrets: ConnectionStrings, API Keys)
```

---

**Documento elaborado**: Enero 2026  
**Versión**: 1.0  
**Autor**: Jorge Ros Gómez
