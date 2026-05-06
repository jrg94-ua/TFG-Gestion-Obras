# Bloque de Memoria: Arquitectura del Sistema, Justificacion Tecnologica y Flujo General de Funcionamiento

## 1. Introduccion

La arquitectura del sistema constituye uno de los elementos centrales del proyecto, ya que condiciona la mantenibilidad, la evolucion del software y la capacidad de integrar modulos funcionales heterogeneos dentro de una misma solucion. En este TFG, la arquitectura no se ha planteado como un modelo teorico abstracto, sino como una respuesta tecnica concreta a un problema real de gestion empresarial en el sector de la construccion.

La solucion desarrollada se materializa en una aplicacion web con autenticacion por roles, persistencia relacional y varios modulos funcionales conectados entre si. Desde el punto de vista arquitectonico, el sistema se articula en una estructura modular basada en proyectos con responsabilidades diferenciadas.

## 2. Arquitectura general del sistema

La solucion se compone de cuatro proyectos principales:

- `GestionObras.Core`
- `GestionObras.Infrastructure`
- `GestionObras.Web`
- `GestionObras.API`

### 2.1 Capa de dominio

`GestionObras.Core` recoge las entidades, enumerados e interfaces que modelan el problema de negocio. En esta capa se representan conceptos como proyecto, tarea, usuario, contrato, material, factura o fichaje. Su funcion es proporcionar una base conceptual estable sobre la que se apoyan la persistencia y la interfaz.

### 2.2 Capa de infraestructura

`GestionObras.Infrastructure` contiene la logica de acceso a datos, con `Entity Framework Core` como tecnologia principal de persistencia. El elemento central es `GestionObrasDbContext`, complementado con repositorios orientados a consultas y operaciones frecuentes sobre proyectos, tareas y recursos humanos.

### 2.3 Capa de presentacion

`GestionObras.Web` constituye el host de presentacion del sistema. Esta capa implementa la interfaz de usuario, el enrutado interno, la navegacion por roles y la experiencia de usuario. En el estado actual del proyecto, su papel principal no es acceder directamente a datos de negocio, sino consumir clientes HTTP tipados hacia la API y coordinar servicios de interfaz o de servidor ligados a `Blazor Server`.

### 2.4 API de negocio

`GestionObras.API` actua como backend de negocio para una parte sustancial de la solucion. Expone endpoints `Minimal API` para proyectos, materiales, solicitudes, RRHH, fichajes, dashboards, empleados, usuarios y tablero personal. Esta API se apoya en DTOs compartidos definidos en `GestionObras.Core/Contracts`, lo que permite desacoplar la interfaz de usuario del modelo de persistencia.

## 3. Organizacion por responsabilidades

Desde una perspectiva conceptual, el sistema puede entenderse como una arquitectura en capas:

- una capa de dominio,
- una capa de persistencia,
- una capa de servicios de aplicacion,
- y una capa de presentacion.

Aunque no existe un proyecto independiente denominado `Application`, ese papel se cubre mediante servicios especializados como `FacturaService`, `PresupuestoService`, `MaterialService`, `DashboardService`, `PlanificacionHorarioService`, `ExportPdfService` y `ExportExcelService`. Estos servicios encapsulan calculos, reglas operativas y transformaciones documentales, reduciendo el acoplamiento entre interfaz y persistencia.
Aunque no existe un proyecto independiente denominado `Application`, ese papel se cubre mediante una combinacion de:

- endpoints y contratos de `GestionObras.API`,
- clientes HTTP tipados en `GestionObras.Web/Services`,
- y servicios especializados como `FacturaService`, `PresupuestoService`, `KanbanService`, `DocumentoService`, `PlanificacionHorarioService`, `RRHHHorariosService`, `ExportPdfService` y `ExportExcelService`.

De este modo, la solucion adopta una arquitectura modular donde la mayor parte del negocio ya se canaliza a traves de API, mientras que ciertas piezas ligadas al host web permanecen locales.

## 4. Justificacion tecnologica

La seleccion de tecnologias responde a un criterio de coherencia tecnica, productividad y adecuacion al alcance de un TFG aplicado.

### 4.1 Ecosistema .NET

La eleccion de `.NET` permite trabajar con un stack unificado para dominio, persistencia, servicios, seguridad y presentacion. Esta homogeneidad reduce la complejidad accidental y facilita construir una aplicacion empresarial completa con un lenguaje y unas herramientas comunes.

### 4.2 Blazor Server

La interfaz se ha implementado con `Blazor Server`, principalmente por:

- reutilizacion del lenguaje C# en toda la solucion,
- integracion directa con `Identity`,
- facilidad para trabajar con servicios del servidor y con autenticacion por cookies,
- y menor coste de integracion que una SPA completamente desacoplada.

Para el tipo de sistema desarrollado, esta decision ofrece una relacion favorable entre esfuerzo de implementacion y valor funcional obtenido.

### 4.3 Entity Framework Core y SQL Server

La persistencia se apoya en `Entity Framework Core` y `SQL Server`. Esta combinacion resulta adecuada porque el dominio requiere integridad referencial, relaciones complejas y consistencia transaccional. `Entity Framework Core` permite, ademas, modelar con naturalidad entidades ricas y consultas con carga relacionada.

### 4.4 ASP.NET Core Identity

La autenticacion y autorizacion se resuelven mediante `ASP.NET Core Identity`, integrado con el contexto de datos. Esta decision evita desarrollar una infraestructura propia de seguridad y cubre de forma robusta requisitos de cuentas, credenciales, roles y restricciones de acceso. Ademas, justifica que `Login`, `Register` y `Logout` permanezcan en el host web, ya que gestionan directamente la cookie de sesion y el ciclo de autenticacion.

### 4.5 QuestPDF y ClosedXML

La generacion documental se implementa con `QuestPDF` para PDF y `ClosedXML` para Excel. Estas librerias permiten transformar la informacion operativa del sistema en salidas documentales utiles para un entorno profesional.

### 4.6 Docker como soporte de despliegue y validacion

El proyecto incorpora contenedorizacion mediante `docker-compose.yml` y `Dockerfile` para la aplicacion web y la API. Docker se utiliza con una doble finalidad:

- facilitar un entorno de ejecucion reproducible,
- y permitir la validacion tecnica del sistema cuando la maquina anfitriona no dispone del SDK requerido.

En consecuencia, Docker forma parte tanto de la estrategia de despliegue como del proceso de aseguramiento tecnico del proyecto.

## 5. Arquitectura de seguridad

La aplicacion implementa una seguridad basada en autenticacion y autorizacion por roles. Los perfiles principales del sistema son:

- `Administrador`,
- `JefeObra`,
- `OficinaTecnica`,
- `RecursosHumanos`,
- `Operario`,
- `OperarioObra`,
- `OperarioOficinaT`.

La navegacion, las pantallas visibles y las operaciones permitidas se filtran segun el rol autenticado. Esta decision reproduce la estructura organizativa de la empresa y evita exponer funcionalidades que no corresponden a cada perfil.

## 6. Flujo general de funcionamiento

El funcionamiento global del sistema puede describirse como una secuencia de interacciones entre usuario, interfaz, servicios y persistencia.

### 6.1 Autenticacion y determinacion de rol

El usuario accede a la aplicacion, introduce sus credenciales y, una vez autenticado, el sistema determina su rol. A partir de ese momento se produce una redireccion al panel correspondiente y se adapta el menu lateral a su perfil. Este tramo sigue siendo local al host web, dado que depende de `ASP.NET Core Identity`.

### 6.2 Acceso a modulos funcionales

Cada rol accede a un subconjunto de modulos:

- administracion: usuarios, catalogos y supervision global,
- jefatura de obra: proyectos, tareas y aprovisionamiento,
- oficina tecnica: seguimiento funcional y documentacion economica,
- recursos humanos: contratos, horarios y fichajes,
- perfiles operativos: tablero personal y fichaje.

En la mayor parte de estos modulos, el componente Razor consume un cliente API tipado, que invoca endpoints de `GestionObras.API` y recibe DTOs listos para representacion.

### 6.3 Gestion de proyectos y tareas

Los proyectos actuan como agregados principales del sistema. Sobre ellos se articulan las tareas, las solicitudes de material, los documentos economicos y parte de la planificacion de recursos humanos. El CRUD de proyectos se resuelve mediante API, mientras que el kanban de proyecto permanece apoyado en un servicio de servidor especializado (`KanbanService`) por su alta densidad de reglas y su fuerte vinculacion con la experiencia interactiva de Blazor Server.

### 6.4 Gestion de materiales

El flujo de materiales permite mantener catalogos, gestionar stock, asociar proveedores y tramitar solicitudes vinculadas a una obra concreta. Este flujo conecta directamente produccion y aprovisionamiento y ya se canaliza a traves de `MaterialesApiClient` y los endpoints correspondientes de la API.

### 6.5 Gestion economica

El bloque economico integra presupuestos y facturas, con servicios locales de calculo y exportacion documental. Esto permite dar soporte a una gestion financiera basica asociada al proyecto, aunque todavia no se ha externalizado como API de negocio del mismo modo que otros modulos.

### 6.6 Recursos humanos

El sistema diferencia entre contratos, horarios planificados y fichajes reales. Esta separacion permite modelar tanto la capacidad prevista como la actividad efectivamente registrada por cada trabajador. Fichajes, contratos, paneles de RRHH y vistas de jefatura ya consumen API, mientras que la planificacion avanzada de horarios sigue apoyandose en servicios locales de aplicacion.

### 6.7 Exportacion documental

Diversos modulos incorporan exportacion a PDF y Excel, lo que extiende la utilidad del sistema mas alla de la simple consulta en pantalla.

## 7. Coherencia arquitectonica

La arquitectura adoptada resulta adecuada para el problema abordado porque:

- separa responsabilidades de forma razonable,
- soporta un dominio con relaciones complejas,
- reproduce la estructura organizativa de la empresa mediante roles,
- y permite integrar modulos funcionales diversos dentro de una misma base de datos y una misma aplicacion.

No se trata de una arquitectura maximalista, sino de una arquitectura pragmatica alineada con el alcance del TFG.

## 8. Limitaciones reconocidas

Desde una lectura critica, deben reconocerse algunas limitaciones:

- la capa de aplicacion no esta aislada en un proyecto independiente,
- algunos modulos especializados, como kanban, planificacion avanzada de horarios, exportacion y autenticacion, siguen resolviendose localmente en el host web,
- no toda la logica de aplicacion se expone aun mediante contratos API,
- y las lineas de inteligencia normativa no se han cerrado funcionalmente.

Estas limitaciones deben interpretarse como decisiones de alcance y priorizacion, no como incoherencias estructurales del sistema.

## 9. Conclusion del bloque

La arquitectura del sistema responde a un criterio de equilibrio entre rigor tecnico, viabilidad de desarrollo y adecuacion al problema resuelto. La combinacion de `Blazor Server`, `ASP.NET Core Identity`, `Minimal API`, `Entity Framework Core`, `SQL Server` y Docker ha permitido construir una solucion coherente, modular y tecnicamente defendible dentro del marco de un TFG de Ingenieria Informatica.
