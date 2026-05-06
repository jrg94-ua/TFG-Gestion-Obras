# Rastreo Integral de la Aplicacion

## Objetivo

Este documento resume el comportamiento real implementado en el codigo de la solucion `TFG-Gestion-Obras`, separando:

- lo que esta operativo en la aplicacion web,
- lo que esta modelado en dominio o base de datos,
- y lo que sigue preparado pero no cerrado funcionalmente.

El objetivo es disponer de una referencia rapida y fiable para el desarrollo del TFG y para futuras sesiones de trabajo sobre el repositorio.

---

## 1. Estado general de la solucion

La solucion esta organizada en cuatro proyectos principales:

- `GestionObras.Core`: entidades, enumerados e interfaces de dominio.
- `GestionObras.Infrastructure`: `DbContext` y repositorios EF Core.
- `GestionObras.Web`: host de presentacion en Blazor Server.
- `GestionObras.API`: backend HTTP de negocio para modulos funcionales de la aplicacion.

### Conclusiones del rastreo

- La aplicacion operativa real se reparte entre `GestionObras.Web` y `GestionObras.API`.
- `GestionObras.Web` usa `ASP.NET Core Identity`, roles y cookies de sesion.
- `GestionObras.API` expone endpoints de negocio reales para proyectos, materiales, RRHH, dashboards, administracion y tableros personales.
- En arranque se crean roles, usuarios base y datos demo si la configuracion lo habilita.

---

## 2. Arquitectura ejecutada realmente

### 2.1 Presentacion

La presentacion esta implementada en Blazor Server con componentes Razor, navegacion por rutas y control de acceso por rol.

Elementos clave:

- `src/GestionObras.Web/Program.cs`
- `src/GestionObras.Web/Components/Routes.razor`
- `src/GestionObras.Web/Components/Layout/NavMenu.razor`

### Comportamiento real

- La ruta `/` redirige al dashboard correspondiente segun el rol autenticado.
- Las rutas usan `AuthorizeRouteView`.
- El menu lateral filtra accesos por roles.
- Los componentes Razor de negocio consumen principalmente clientes HTTP tipados hacia la API o servicios locales de aplicacion.
- La autenticacion y el alta/baja de sesion siguen resolviendose localmente en el host web por su dependencia de `Identity`.

### 2.2 Dominio y persistencia

El modelo de datos central esta en:

- `src/GestionObras.Infrastructure/Data/GestionObrasDbContext.cs`

Entidades principales:

- `Proyecto`
- `Tarea`
- `BloqueoTarea`
- `DocumentoTarea`
- `FirmaTarea`
- `UsuarioObra`
- `Empleado`
- `Material`
- `CategoriaMaterial`
- `SolicitudMaterial`
- `Factura`
- `Presupuesto`
- `Proveedor`
- `RegistroFichaje`
- `HorarioAsignado`
- `Contrato`
- `CarpetaLegal`

### Relaciones relevantes

- `Proyecto` tiene muchas `Tareas`.
- `Proyecto` tiene `Presupuesto` y `CarpetaLegal` en relacion uno a uno.
- `Tarea` soporta jerarquia padre-subtarea.
- `Tarea` soporta dependencias entre tareas mediante `TareaDependencias`.
- `Tarea` soporta asignacion multiple de usuarios.
- `Tarea` soporta responsable final, documentos, bloqueos y firmas.
- `Material` soporta proveedor principal y relacion muchos a muchos con proveedores.
- `SolicitudMaterial` relaciona material, proyecto, solicitante y revisor.
- `RegistroFichaje` relaciona usuario, proyecto y estado de validacion.
- `HorarioAsignado` relaciona usuario, proyecto, dia y turno.
- `Contrato` relaciona usuario con condiciones laborales.

---

## 3. Roles implementados

Los roles creados y usados por la aplicacion son:

- `Administrador`
- `JefeObra`
- `OficinaTecnica`
- `Operario`
- `OperarioObra`
- `OperarioOficinaT`
- `RecursosHumanos`

### Flujo de entrada

- `Login.razor` autentica con `SignInManager`.
- `Home.razor` redirige por rol.
- `Logout.razor` cierra sesion.
- `Register.razor` permite registrar usuarios y asignar rol, con mayor alcance si el usuario actual es administrador.

### Paneles por rol

- `Administrador`: `/admin/dashboard`
- `JefeObra`: `/jefe-obra/dashboard`
- `OficinaTecnica`: `/oficina-tecnica/dashboard`
- `RecursosHumanos`: `/rrhh/dashboard`
- `Operarios`: `/mi-tablero`

---

## 4. Modulos funcionales operativos

### 4.1 Dashboard y navegacion

Pantallas:

- `Dashboard.razor`
- `AdminDashboard.razor`
- `JefeObraDashboard.razor`
- `OficinaTecnicaDashboard.razor`
- `RRHHDashboard.razor`

Funcionalidad:

- metricas agregadas,
- accesos rapidos por rol,
- resumenes de proyectos, contratos y fichajes,
- alertas basicas.

Flujo tecnico actual:

- `Dashboard.razor`, `AdminDashboard.razor`, `JefeObraDashboard.razor` y `OficinaTecnicaDashboard.razor` consumen `ConsultasApiClient`.
- `RRHHDashboard.razor` consume `RRHHApiClient`.

### 4.2 Gestion de proyectos

Pantallas:

- `Proyectos.razor`
- `ProyectoDetalleRedirect.razor`
- `Admin/TableroProyectos.razor`

Funcionalidad real:

- alta, edicion y eliminacion de proyectos,
- filtro por estado y busqueda,
- asignacion de responsable,
- vista lista y vista temporal agregada,
- tablero global de proyectos para administrador con drag and drop entre estados,
- navegacion directa a kanban, gantt e historial de materiales.

Observacion:

- la ruta `/proyectos/{id}` no tiene pagina de detalle propia; redirige al kanban del proyecto.
- `Proyectos.razor` consume `ProyectosApiClient`, que delega en la API el CRUD y la consulta filtrada.

### 4.3 Kanban de proyecto

Pantalla principal:

- `Kanban.razor`

Es uno de los modulos mas completos del sistema.

Funcionalidad operativa:

- CRUD de tareas.
- Creacion de subtareas.
- Drag and drop entre estados.
- Asignacion multiple de usuarios responsables.
- Asignacion de `ResponsableFinal`.
- Definicion de dependencias entre tareas predecesoras.
- Reglas de negocio para impedir avances inconsistentes.
- Bloqueo y desbloqueo de tareas con justificacion.
- Propagacion de bloqueo a subtareas descendientes en ciertos casos.
- Subida y eliminacion de documentos adjuntos.
- Cierre de tarea con observaciones de finalizacion.
- Firma conjunta en tareas colaborativas.
- Bloqueo automatico si una firma rechaza la tarea.
- Exportacion del tablero a PDF y Excel.

Reglas funcionales detectadas:

- una tarea no puede avanzar si tiene predecesoras no finalizadas,
- una tarea padre no puede finalizar si conserva subtareas abiertas,
- una subtarea no puede avanzar si su padre esta pendiente o bloqueado,
- una subtarea no puede reabrirse si la tarea padre ya esta finalizada,
- una tarea con firma conjunta no puede completarse hasta reunir todas las firmas aprobadas.

### 4.4 Tablero personal

Pantalla:

- `MiTablero.razor`

Funcionalidad:

- vista personal de tareas asignadas,
- cambio de estado por columnas,
- bloqueo y desbloqueo,
- edicion rapida,
- marcado manual como finalizada,
- exportacion PDF y Excel.

Es una vista simplificada respecto al kanban completo de proyecto.

Flujo tecnico actual:

- `MiTablero.razor` consume `AdministracionApiClient`.
- La API expone cambios de estado, bloqueo, desbloqueo y finalizacion para el tablero personal.

### 4.5 Gantt de proyecto

Pantalla:

- `GanttProyecto.razor`

Funcionalidad:

- visualizacion temporal tipo gantt,
- zoom,
- scroll sincronizado,
- filtros,
- detalle de tarea en modal.

No se detecta edicion de planificacion mediante arrastre sobre el diagrama; su foco actual es visualizacion y seguimiento.

Flujo tecnico actual:

- `GanttProyecto.razor` consume `ConsultasApiClient`.

### 4.6 Empleados y usuarios

Pantallas:

- `Empleados.razor`
- `Admin/Usuarios.razor`

Funcionalidad:

- gestion de empleados,
- alta y edicion de usuarios asociados,
- relacion entre empleado y cuenta `UsuarioObra`,
- baja logica de usuarios al eliminar empleados,
- gestion completa de usuarios y roles desde administrador,
- exportacion PDF y Excel de empleados.

Flujo tecnico actual:

- `Empleados.razor`, `Admin/Usuarios.razor` y `Admin/TableroProyectos.razor` consumen `AdministracionApiClient`.

### 4.7 Materiales y catalogos

Pantallas:

- `Materiales.razor`
- `Admin/Catalogos.razor`

Funcionalidad:

- CRUD de materiales,
- control de stock y stock minimo,
- proveedor principal,
- relacion con multiples proveedores,
- activacion y desactivacion,
- gestion administrativa de proveedores,
- gestion administrativa de categorias de material.

Flujo tecnico actual:

- `Materiales.razor`, `Admin/Catalogos.razor`, `JefeObra/SolicitarMateriales.razor`, `JefeObra/MisSolicitudesMateriales.razor` y `Admin/GestionSolicitudesMateriales.razor` consumen `MaterialesApiClient`.

### 4.8 Solicitudes de materiales

Pantallas:

- `JefeObra/SolicitarMateriales.razor`
- `JefeObra/MisSolicitudesMateriales.razor`
- `Admin/GestionSolicitudesMateriales.razor`
- `HistorialMaterialesProyecto.razor`

Funcionalidad:

- el jefe de obra crea solicitudes sobre materiales y proyectos,
- consulta el estado de sus solicitudes,
- puede cancelar solicitudes pendientes,
- el administrador aprueba o rechaza,
- la aprobacion valida disponibilidad de stock,
- la aprobacion descuenta stock del material,
- cada proyecto puede consultar el historial economico y operativo de solicitudes.

### 4.9 Presupuestos y facturas

Pantallas:

- `Presupuestos.razor`
- `Facturas.razor`

Servicios:

- `PresupuestoService`
- `FacturaService`
- `ExportPdfService`
- `ExportExcelService`

Funcionalidad:

- CRUD de presupuestos,
- CRUD de facturas,
- calculo automatico de base neta, IVA, descuento e importe total en facturas,
- exportacion PDF y Excel de listados,
- exportacion PDF y Excel de elementos individuales.

### 4.10 Fichajes

Pantallas:

- `Operario/Fichaje.razor`
- `JefeObra/ControlFichajes.razor`
- `RRHH/RRHHFichajes.razor`

Repositorio:

- `FichajeRepository`

Funcionalidad operativa:

- fichaje de entrada,
- fichaje de salida,
- asociacion opcional a proyecto,
- consulta de historial reciente,
- validacion y rechazo de fichajes desde RRHH,
- visualizacion de fichajes del equipo para jefe de obra y oficina tecnica en modo solo lectura,
- exportacion PDF y Excel desde RRHH.

Importante:

- el modelo `RegistroFichaje` soporta latitud y longitud de entrada y salida,
- la interfaz actual no captura geolocalizacion todavia,
- por tanto el requisito esta parcialmente soportado a nivel de modelo, pero no cerrado en UI.

Flujo tecnico actual:

- `Operario/Fichaje.razor` consume `OperarioApiClient`.
- `JefeObra/ControlFichajes.razor` consume `JefeObraApiClient`.
- `RRHH/RRHHFichajes.razor` consume `RRHHApiClient`.

### 4.11 Horarios

Pantallas:

- `JefeObra/GestionHorarios.razor`
- `RRHH/RRHHHorarios.razor`

Servicios:

- `PlanificacionHorarioService`

Funcionalidad:

- consulta de horarios del equipo para jefe de obra y oficina tecnica,
- gestion completa de horarios desde RRHH,
- alta, edicion y eliminacion de asignaciones,
- exportacion PDF y Excel,
- generacion automatica de horarios por proyecto.

La generacion automatica usa:

- carga semanal estimada de tareas,
- trabajador final asignado,
- contratos activos,
- limite de horas semanales contractuales.

Flujo tecnico actual:

- `JefeObra/GestionHorarios.razor` consume `JefeObraApiClient`.
- `RRHH/RRHHHorarios.razor` consume `RRHHHorariosService` y `PlanificacionHorarioService`.

### 4.12 Contratos

Pantalla:

- `RRHH/RRHHContratos.razor`

Funcionalidad:

- alta y edicion de contratos,
- finalizacion de contratos,
- consulta filtrada,
- exportacion PDF y Excel,
- soporte de jornada, tipo de contrato, salario y centro de trabajo.

---

## 5. Servicios transversales implementados

### 5.1 `DashboardService`

Agrega estadisticas generales del sistema.

### 5.2 `DocumentoService`

Gestiona:

- almacenamiento fisico de documentos de tarea,
- persistencia de metadatos,
- recuperacion,
- borrado.

### 5.3 `ExportPdfService`

Genera informes PDF de:

- facturas,
- presupuestos,
- empleados,
- tareas,
- contratos,
- horarios,
- fichajes.

### 5.4 `ExportExcelService`

Genera versiones Excel de los mismos conjuntos de datos anteriores.

### 5.5 `DemoDataSeeder`

Genera dataset de demostracion amplio:

- usuarios por rol,
- empleados,
- proveedores,
- materiales,
- proyectos,
- tareas,
- facturas,
- solicitudes,
- horarios,
- fichajes,
- contratos.

Esto convierte la aplicacion en una demo funcional bastante completa desde el arranque.

---

## 6. Funcionalidades modeladas o parciales

Durante el rastreo se han identificado piezas que existen en entidad, interfaz o esquema, pero no estan cerradas como modulo funcional final.

### 6.1 Carpeta legal y normativa

Existe soporte de modelo para:

- `CarpetaLegal`,
- referencias a `DocumentoCTE`,
- `DocumentoLOTUP`,
- `DocumentoPGOU`,
- `ParametrosUrbanisticosJson`.

Tambien existen interfaces en `IServiciosNormativos.cs` para:

- normativa estatal,
- catalogo de materiales CTE,
- normativa autonomica,
- inteligencia local sobre PGOU.

Sin embargo:

- no se ha detectado implementacion funcional de esos servicios,
- no hay un modulo operativo completo de carpeta legal,
- `/admin/carpetas-legales` esta marcado como en construccion,
- la inteligencia normativa descrita en la documentacion general aun no esta integrada en la app ejecutable.

### 6.2 API de negocio

`GestionObras.API` implementa endpoints de negocio del TFG para una parte significativa del sistema.

Estado real:

- actua como backend para jefatura de obra, operarios, RRHH, proyectos, materiales, consultas y administracion,
- usa `Minimal API` con DTOs compartidos en `GestionObras.Core/Contracts`,
- y se consume desde `GestionObras.Web` mediante clientes HTTP tipados.

### 6.3 Validacion PRL real en flujos

El dominio `Empleado` incluye cursos PRL y una validacion `TienePRLVigente()`.

Estado real:

- la idea esta modelada,
- pero no se detecta un flujo de interfaz completo donde la PRL condicione altas, asignaciones o acceso operativo de forma cerrada.

### 6.4 Geolocalizacion de fichaje

Estado real:

- soportada por entidad y esquema,
- no capturada en la UI actual.

---

## 7. Diferencia entre documentacion conceptual y aplicacion real

La documentacion de alto nivel del repositorio describe una vision mas ambiciosa del TFG, con:

- automatizacion normativa basada en BOE y CTE,
- busqueda de PGOU con IA,
- RAG local con Llama 3,
- carpeta legal automatica.

Tras el rastreo del codigo, el estado real actual es:

- muy fuerte en operativa interna de obra,
- fuerte en kanban, materiales, RRHH, facturas y presupuestos,
- parcial en normativa automatizada y geolocalizacion,
- con arquitectura predominantemente API-first en modulos de negocio, aunque todavia hibrida en autenticacion y algunos servicios locales especializados.

En otras palabras, el producto actual ya funciona como sistema interno de gestion de obra y personal, pero no ha cerrado todavia toda la capa de inteligencia normativa prometida en la vision del TFG.

---

## 8. Resumen ejecutivo

### Modulos claramente operativos

- autenticacion y roles,
- dashboards por perfil,
- gestion de proyectos,
- kanban de proyecto,
- tablero personal,
- gantt de proyecto,
- empleados y usuarios,
- materiales y catalogos,
- solicitudes de materiales,
- presupuestos,
- facturas,
- fichajes,
- horarios,
- contratos,
- exportacion PDF y Excel,
- carga de demo completa.

### Modulos parciales o pendientes

- carpeta legal automatica,
- servicios reales de normativa BOE y CTE,
- IA sobre PGOU,
- geolocalizacion efectiva en fichaje,
- PRL integrada end-to-end en flujos operativos.

---

## 9. Uso recomendado de este documento

Este rastreo puede usarse como base para:

- memoria tecnica del TFG,
- defensa funcional,
- planificacion de siguientes sprints,
- alineacion entre documentacion y estado real del repositorio,
- generacion posterior de manuales por rol.

Si el proyecto sigue evolucionando, conviene mantener este documento sincronizado con cada entrega relevante.
