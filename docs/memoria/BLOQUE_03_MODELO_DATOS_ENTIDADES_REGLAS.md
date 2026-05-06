# Bloque de Memoria: Modelo de Datos, Diseno de Entidades y Reglas de Negocio

## 1. Introduccion

El modelo de datos constituye una de las piezas mas relevantes del proyecto, ya que traduce el problema del dominio a una estructura persistente capaz de sostener la operativa del sistema. En una aplicacion de gestion de obras, la informacion no puede entenderse como un conjunto de registros aislados, sino como una red de relaciones entre proyectos, tareas, personas, materiales, documentos y procesos administrativos.

Desde el punto de vista academico, el modelo de datos aporta valor porque evidencia el trabajo de analisis del dominio y la capacidad de estructurar una solucion coherente a partir de entidades, relaciones y restricciones.

## 2. Enfoque general del modelo

La persistencia se implementa mediante una base de datos relacional gestionada con `SQL Server` y modelada a traves de `Entity Framework Core`. Esta eleccion resulta adecuada porque el dominio presenta:

- entidades con identidad clara,
- relaciones uno a uno, uno a muchos y muchos a muchos,
- necesidad de integridad referencial,
- y operaciones donde la consistencia de los datos es critica.

El modelo se centraliza en `GestionObrasDbContext`, donde se configuran las entidades principales y sus relaciones.

## 3. Agrupacion funcional de entidades

El conjunto de entidades del sistema puede agruparse en cuatro bloques principales.

### 3.1 Proyectos y produccion

Este bloque recoge el nucleo operativo de la aplicacion:

- `Proyecto`,
- `Tarea`,
- `BloqueoTarea`,
- `DocumentoTarea`,
- `FirmaTarea`.

### 3.2 Usuarios y recursos humanos

En este grupo se integran las entidades relacionadas con acceso, condiciones laborales y actividad diaria:

- `UsuarioObra`,
- `Empleado`,
- `Contrato`,
- `HorarioAsignado`,
- `RegistroFichaje`.

### 3.3 Materiales y aprovisionamiento

Este bloque modela la gestion de recursos materiales:

- `Material`,
- `CategoriaMaterial`,
- `Proveedor`,
- `SolicitudMaterial`.

### 3.4 Economia y documentacion

Finalmente, el sistema incorpora entidades de soporte economico y documental:

- `Presupuesto`,
- `Factura`,
- `CarpetaLegal`.

## 4. Entidades principales

### 4.1 Proyecto

`Proyecto` es el agregado principal del sistema. Sobre esta entidad pivotan la planificacion, la gestion de tareas, las solicitudes de material, los documentos economicos y parte del control de recursos humanos. Sus atributos recogen identificacion, localizacion, responsable, estado y relaciones con presupuesto y carpeta legal.

### 4.2 Tarea

`Tarea` representa la unidad operativa principal dentro de un proyecto. Su diseno soporta:

- jerarquia padre-subtarea,
- dependencias entre tareas,
- asignacion multiple de usuarios,
- responsable final,
- bloqueo,
- firma conjunta,
- costes e informacion temporal,
- y documentacion adjunta.

Es la entidad con mayor densidad funcional del sistema y la que concentra una parte sustancial de las reglas de negocio.

### 4.3 UsuarioObra, Empleado y Contrato

Una decision relevante del modelo ha sido distinguir entre:

- `UsuarioObra`, como identidad digital y acceso al sistema,
- `Empleado`, como representacion administrativa de la persona,
- `Contrato`, como estructura de relacion laboral.

Esta separacion evita mezclar credenciales, datos laborales y comportamiento funcional en una unica entidad.

### 4.4 HorarioAsignado y RegistroFichaje

`HorarioAsignado` representa la planificacion prevista, mientras que `RegistroFichaje` modela la actividad real registrada por el trabajador. Esta diferenciacion es especialmente importante, ya que permite comparar planificacion y ejecucion.

### 4.5 Material, Proveedor y SolicitudMaterial

`Material` se relaciona con categorias y proveedores, y sirve de base para el flujo de solicitudes internas. `SolicitudMaterial` conecta proyecto, material, solicitante, revisor y estado del proceso de aprobacion.

### 4.6 Presupuesto y Factura

`Presupuesto` actua como referencia economica del proyecto y `Factura` permite registrar documentos financieros con base imponible, descuento, IVA, estado y relacion con proyecto y proveedor.

## 5. Relaciones principales

El valor del modelo no reside unicamente en las entidades, sino tambien en sus relaciones.

### 5.1 Relaciones de proyecto

- un proyecto tiene muchas tareas,
- un proyecto tiene un presupuesto en relacion uno a uno,
- un proyecto tiene una carpeta legal en relacion uno a uno,
- un proyecto tiene un responsable.

### 5.2 Relaciones de tarea

- cada tarea pertenece a un proyecto,
- puede tener tarea padre y subtareas,
- puede depender de otras tareas,
- puede tener varios usuarios asignados,
- puede tener un responsable final,
- puede incorporar documentos y firmas.

### 5.3 Relaciones de RRHH

- un usuario puede tener contratos sucesivos,
- un usuario puede tener multiples horarios con distintas vigencias,
- un usuario puede tener multiples fichajes asociados a distintas jornadas y proyectos.

### 5.4 Relaciones de materiales y economia

- un material puede vincularse a varios proveedores,
- una solicitud conecta material, proyecto y usuarios implicados,
- una factura puede asociarse a proveedor y proyecto.

## 6. Reglas de negocio apoyadas por el modelo

El modelo de datos no solo almacena informacion, sino que soporta reglas operativas reales.

### 6.1 Reglas de tareas

La estructura de `Tarea` permite imponer restricciones como:

- impedir avances si existen dependencias pendientes,
- impedir el cierre de tareas padre con subtareas abiertas,
- controlar tareas colaborativas con firma conjunta,
- y registrar bloqueos con trazabilidad.

### 6.2 Reglas de horarios

La relacion entre `Contrato`, `HorarioAsignado` y `UsuarioObra` permite aplicar restricciones sobre:

- capacidad semanal segun contrato,
- limite diario de horas,
- perfiles operativos asignables,
- y deficit de cobertura en la planificacion.

### 6.3 Reglas de trazabilidad

Entidades como `DocumentoTarea`, `Factura`, `Presupuesto` y `SolicitudMaterial` refuerzan la trazabilidad de procesos y decisiones dentro del sistema.

## 7. Decisiones de diseno destacables

Entre las decisiones de modelado mas relevantes se encuentran:

- el uso de una base de datos relacional para un dominio fuertemente conectado,
- la integracion de `Identity` dentro del mismo contexto de datos,
- la separacion entre identidad digital y datos laborales,
- y el modelado temporal de los horarios mediante vigencias.

Estas decisiones mejoran la coherencia conceptual del sistema y facilitan la evolucion futura del software.

## 8. Valor academico del modelo

Desde la perspectiva de un TFG, el modelo de datos aporta valor porque:

- demuestra un analisis del dominio no trivial,
- integra varias areas funcionales en una misma estructura coherente,
- conecta entidades con reglas de negocio reales,
- y sostiene una aplicacion que va mas alla de un CRUD elemental.

## 9. Limitaciones y lectura critica

El modelo cubre con solidez el nucleo funcional del sistema. No obstante, algunas lineas inicialmente previstas no han alcanzado el mismo nivel de madurez, especialmente las relacionadas con inteligencia normativa y determinadas validaciones avanzadas. Esta circunstancia debe presentarse como una limitacion de alcance y no como una debilidad del modelo principal, que si resulta suficiente para sostener la aplicacion realmente implementada.

## 10. Conclusion del bloque

El modelo de datos del sistema refleja una aproximacion seria y estructurada al problema de gestion de obras. Su diseno permite representar proyectos, tareas, personas, materiales, economia y recursos humanos dentro de un mismo marco relacional, y constituye una evidencia clara del trabajo de analisis y modelado realizado a lo largo del TFG.
