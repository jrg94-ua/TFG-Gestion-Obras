# Arquitectura Real Implementada

## Objetivo

Este documento describe la arquitectura realmente ejecutada por la aplicacion, diferenciandola de la arquitectura objetivo o ideal. Su funcion es servir como base para la memoria del TFG y para la defensa tecnica del sistema.

## 1. Vision general

La solucion se organiza en cuatro proyectos:

- `GestionObras.Core`: entidades, enumerados e interfaces del dominio.
- `GestionObras.Infrastructure`: persistencia EF Core y repositorios.
- `GestionObras.Web`: host de presentacion en Blazor Server.
- `GestionObras.API`: backend HTTP de negocio para la mayor parte de modulos operativos.

La aplicacion operativa se reparte entre `GestionObras.Web` y `GestionObras.API`. El usuario interactua con componentes Razor renderizados en servidor, autenticados con `ASP.NET Core Identity`, mientras que los casos de uso de negocio se resuelven de forma predominante a traves de clientes HTTP tipados hacia la API. La persistencia sigue apoyandose en `SQL Server` y `Entity Framework Core`.

## 2. Estilo arquitectonico aplicado

El proyecto sigue una separacion de responsabilidades cercana a una arquitectura por capas:

- capa de presentacion,
- capa de dominio,
- capa de infraestructura,
- capa de servicios de aplicacion localizada principalmente en `GestionObras.Web/Services`.

No existe una capa `Application` independiente como proyecto separado. En la implementacion real, parte de la orquestacion de casos de uso esta en:

- endpoints y contratos de `GestionObras.API`,
- clientes HTTP tipados en `GestionObras.Web/Services`,
- servicios de servidor como `KanbanService`, `DocumentoService`, `FacturaService`, `PresupuestoService` y `PlanificacionHorarioService`,
- repositorios en `Infrastructure`,
- y componentes Razor centrados ya en coordinacion de interfaz, estado y eventos.

## 3. Capa de presentacion

La presentacion se resuelve con `Blazor Server`.

Elementos principales:

- `src/GestionObras.Web/Program.cs`
- `src/GestionObras.Web/Components/Routes.razor`
- `src/GestionObras.Web/Components/Layout/NavMenu.razor`
- `src/GestionObras.Web/Components/Pages/*`

### Responsabilidades

- autenticacion de usuarios y mantenimiento de cookie de sesion,
- enrutado por paginas Razor,
- autorizacion por roles,
- consumo de clientes API y servicios locales especializados,
- exportacion documental mediante JS interop,
- experiencia diferenciada por perfil.

### Implicacion arquitectonica

La eleccion de `Blazor Server` simplifica el desarrollo full stack en .NET y mantiene la autenticacion y la sesion dentro del host web. Al mismo tiempo, la solucion ha evolucionado hacia un modelo API-first para la logica de negocio principal, evitando que los componentes Razor queden acoplados de forma directa al acceso a datos.

## 4. Seguridad y control de acceso

La autenticacion y autorizacion se apoya en:

- `ASP.NET Core Identity`,
- roles persistidos en base de datos,
- politicas registradas en `Program.cs`,
- proteccion por `Authorize` en paginas y componentes.

Roles detectados en el sistema:

- `Administrador`
- `JefeObra`
- `OficinaTecnica`
- `Operario`
- `OperarioObra`
- `OperarioOficinaT`
- `RecursosHumanos`

La navegacion lateral no es solo visual: sirve tambien como mecanismo de reduccion de superficie funcional por perfil, evitando que determinados roles accedan a modulos ajenos a su operativa.

## 5. Persistencia y acceso a datos

La persistencia se centraliza en `GestionObrasDbContext`, que extiende `IdentityDbContext<UsuarioObra>`.

Puntos clave:

- almacenamiento de usuarios y roles en el mismo contexto que el dominio,
- mapeo de relaciones complejas en `OnModelCreating`,
- uso de relaciones uno a uno, uno a muchos, muchos a muchos y autorreferencias,
- uso de `EnsureCreatedAsync` y ajustes complementarios de esquema en arranque.

### Consecuencia tecnica

El sistema prioriza rapidez de evolucion y coherencia interna sobre una estrategia de versionado de base de datos mas formal. Para un TFG esto es defendible si se explica como una decision pragmatica orientada a prototipo funcional avanzado.

## 6. Servicios de aplicacion reales

Los servicios implementados y usados por la aplicacion son:

- `FacturaService`
- `PresupuestoService`
- `MaterialService`
- `PlanificacionHorarioService`
- `KanbanService`
- `DocumentoService`
- `RRHHHorariosService`
- `ExportPdfService`
- `ExportExcelService`

Ademas, la capa web consume clientes HTTP tipados como:

- `JefeObraApiClient`
- `OperarioApiClient`
- `RRHHApiClient`
- `ProyectosApiClient`
- `MaterialesApiClient`
- `ConsultasApiClient`
- `AdministracionApiClient`

### Papel de estos servicios

- encapsular operaciones de consulta, persistencia y coordinacion UI repetidas,
- centralizar reglas de calculo economico,
- concentrar logica de planificacion de horarios,
- resolver flujos de tablero kanban y gestion documental local,
- consumir la API de negocio con contratos tipados,
- generar documentos PDF y Excel,
- reducir acoplamiento directo entre UI y `DbContext`.

La situacion actual ya no es la de paginas Razor consultando datos de negocio de forma generalizada. El acceso directo ha sido sustituido, en la mayor parte de modulos, por clientes API o por servicios locales de aplicacion.

## 7. Repositorios implementados

Repositorios relevantes:

- `IProyectoRepository / ProyectoRepository`
- `ITareaRepository / TareaRepository`
- `IEmpleadoRepository / EmpleadoRepository`
- `IFichajeRepository / FichajeRepository`

### Uso real

El patron repositorio no se aplica de forma uniforme en todo el sistema. Convive con servicios locales que acceden al `DbContext`, con endpoints minimal API y con clientes HTTP en la capa web. La arquitectura resultante es modular y defendible, aunque no corresponde a una `Clean Architecture` estricta con capa `Application` separada.

## 8. Integracion y despliegue

El sistema esta preparado para ejecutarse en contenedores Docker con:

- aplicacion web,
- aplicacion API,
- SQL Server.

La documentacion de despliegue ya existente permite defender una capacidad real de ejecucion reproducible, que suma valor academico porque demuestra operatividad mas alla del codigo fuente.

## 9. Limitaciones arquitectonicas detectadas

- La capa de aplicacion no esta aislada como proyecto independiente.
- Conviven varios estilos de acceso a datos: repositorio, servicio local y endpoints minimal API.
- La autenticacion y parte de los servicios especializados siguen resolviendose localmente en `GestionObras.Web`.
- La estrategia de evolucion de esquema se apoya en inicializacion y SQL auxiliar, no en una politica formal de migraciones explicada como parte central del sistema.
- Modulos como kanban, exportacion y autenticacion no estan expuestos aun como API publica.

## 10. Valor para la memoria del TFG

Esta arquitectura es suficiente para defender:

- separacion funcional por capas,
- uso de tecnologias coherentes con el problema,
- autenticacion y autorizacion robustas,
- persistencia rica en relaciones,
- una capa API real para la mayor parte del negocio,
- y una progresion razonable desde un sistema monolitico modular hacia una arquitectura mas desacoplada.

La clave en la memoria no es presentarla como una arquitectura perfecta, sino como una arquitectura pragmatica, coherente con el alcance y con una evolucion justificable durante el desarrollo.
