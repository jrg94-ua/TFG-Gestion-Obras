# Decisiones Arquitectonicas del Proyecto

## Objetivo

Este documento recoge decisiones tecnicas relevantes del proyecto, el problema que resolvian y la justificacion de por que se eligio una solucion concreta.

## DA-01. Uso de Blazor Server como interfaz principal

### Problema

El sistema necesitaba una interfaz rica, con cambios por rol, formularios intensivos, tablas, modales y tableros interactivos, sin multiplicar la complejidad tecnologica.

### Decision

Implementar la aplicacion principal en `Blazor Server`.

### Motivos

- unifica frontend y backend en el ecosistema .NET,
- reduce tiempo de desarrollo,
- facilita acceso directo a autenticacion, autorizacion y servicios internos,
- permite iterar rapido sobre una aplicacion de gestion empresarial.

### Coste asumido

- mayor peso del servidor en la interaccion,
- menor separacion frontend/backend que en una SPA desacoplada.

## DA-02. Evolucionar hacia una API de negocio reutilizable

### Problema

El proyecto arranco con la prioridad de cerrar primero una aplicacion usable, pero a medida que crecieron los modulos aparecio la necesidad de desacoplar la UI del acceso a datos y reutilizar casos de uso.

### Decision

Convertir `GestionObras.API` en backend HTTP para la mayor parte del negocio, manteniendo en `GestionObras.Web` solo la autenticacion y ciertos servicios de servidor especialmente ligados a Blazor Server.

### Motivos

- reduce acoplamiento entre componentes Razor y persistencia,
- hace mas defendible la arquitectura del TFG,
- permite contratos compartidos y clientes HTTP tipados,
- deja preparada la aplicacion para evolucionar a otros consumidores o frontends.

### Implicacion para la memoria

Debe presentarse como una evolucion arquitectonica real ya materializada en proyectos, materiales, RRHH, dashboards, administracion y tableros personales. La parte que sigue local no es el negocio principal, sino autenticacion y algunos servicios especializados del host web.

## DA-03. Contexto de datos unico con Identity integrado

### Problema

El sistema necesitaba gestionar usuarios, roles, proyectos, tareas, materiales, facturas y RRHH con coherencia transaccional.

### Decision

Usar `GestionObrasDbContext` como contexto central, heredando de `IdentityDbContext<UsuarioObra>`.

### Motivos

- simplifica autenticacion y autorizacion,
- evita duplicidad de datos de usuario,
- facilita el seed de cuentas demo,
- reduce friccion tecnica en un proyecto de tiempo limitado.

## DA-04. Modelo de tareas jerarquico y con dependencias

### Problema

La gestion de obra no podia representarse bien con una lista plana de tareas.

### Decision

Permitir:

- tarea padre y subtareas,
- dependencias entre tareas,
- asignacion multiple,
- responsable final,
- bloqueo y firma.

### Motivos

- modela mejor la realidad de la produccion,
- permite reglas de avance mas ricas,
- da profundidad funcional al kanban.

### Resultado

Este modulo se convierte en el nucleo diferenciador del sistema frente a una gestion de proyectos basica.

## DA-05. Separar horarios previstos de fichajes reales

### Problema

Planificar trabajo y registrar presencia son dos procesos distintos.

### Decision

Mantener entidades separadas:

- `HorarioAsignado` para planificacion,
- `RegistroFichaje` para ejecucion real.

### Motivos

- evita mezclar prevision con evidencia,
- facilita validacion e incidencias,
- permite ampliar el sistema hacia control horario mas estricto.

## DA-06. Usar contratos como restriccion de capacidad

### Problema

No todos los trabajadores tienen la misma disponibilidad semanal y no todos los perfiles deben planificarse como personal operativo de obra.

### Decision

Usar `Contrato.HorasSemanales` y `TipoUsuario` como base de la capacidad de planificacion.

### Motivos

- alinea la generacion automatica con la logica del negocio,
- evita tratar a todo trabajador como operario full time,
- permite justificar deficits de capacidad y necesidad de plantilla adicional.

## DA-07. Introducir servicios de aplicacion especificos

### Problema

Los componentes Razor concentraban demasiada logica de datos y calculo.

### Decision

Extraer parte de la logica a servicios como:

- `FacturaService`,
- `PresupuestoService`,
- `MaterialService`,
- `KanbanService`,
- `PlanificacionHorarioService`.

### Motivos

- mejora mantenibilidad,
- permite reutilizar calculos,
- reduce acoplamiento entre UI y persistencia,
- hace la arquitectura mas defendible academicamente.

## DA-08. Exportacion documental integrada

### Problema

La aplicacion debia generar evidencias operativas utiles para gestion y defensa del TFG.

### Decision

Integrar exportacion a PDF y Excel con `QuestPDF` y `ClosedXML`.

### Motivos

- aporta valor real de negocio,
- permite presentar salidas tangibles del sistema,
- mejora la calidad percibida del producto final.

## DA-09. Seed de datos demo y arranque reproducible

### Problema

El proyecto necesitaba poder demostrarse de forma inmediata.

### Decision

Inicializar roles, usuarios y datos demo desde arranque cuando la configuracion lo habilita.

### Motivos

- facilita pruebas,
- simplifica la demo,
- mejora reproducibilidad del entorno.

## DA-10. Tratamiento explicito de limitaciones

### Problema

El proyecto tiene un alcance amplio y no todas las lineas iniciales se han cerrado al mismo nivel.

### Decision

Documentar con claridad:

- que la API ya soporta buena parte del negocio,
- que la autenticacion sigue local por integracion con `Identity`,
- que la inteligencia normativa esta modelada pero no terminada,
- y que existen lineas futuras de desacoplamiento, pruebas y automatizacion.

### Motivos

- mejora la honestidad tecnica,
- protege la defensa frente a preguntas del tribunal,
- convierte limitaciones en decisiones de alcance justificadas.
