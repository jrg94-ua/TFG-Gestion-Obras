# Modelo de Datos y Entidades

## Objetivo

Este documento resume la estructura real de datos del sistema a partir de `GestionObrasDbContext` y de las entidades del proyecto `GestionObras.Core`.

## 1. Entidades principales

### Nucleo de produccion y planificacion

- `Proyecto`
- `Tarea`
- `BloqueoTarea`
- `DocumentoTarea`
- `FirmaTarea`

### Personas y acceso

- `UsuarioObra`
- `Empleado`
- `Contrato`
- `RegistroFichaje`
- `HorarioAsignado`

### Materiales y compras

- `Material`
- `CategoriaMaterial`
- `Proveedor`
- `SolicitudMaterial`

### Gestion economica y documental

- `Factura`
- `Presupuesto`
- `CarpetaLegal`

## 2. Relaciones clave del modelo

### Proyecto

`Proyecto` actua como agregado principal del sistema.

Relaciones detectadas:

- un proyecto tiene muchas tareas,
- un proyecto tiene un responsable funcional,
- un proyecto tiene un presupuesto en relacion uno a uno,
- un proyecto tiene una carpeta legal en relacion uno a uno,
- un proyecto puede relacionarse con fichajes, horarios, solicitudes de material y facturas.

### Tarea

`Tarea` es la entidad operativa central del sistema.

Relaciones y capacidades:

- pertenece a un proyecto,
- soporta jerarquia padre-subtarea,
- soporta dependencias entre tareas,
- permite asignacion multiple de usuarios,
- tiene un responsable final,
- puede ser bloqueada,
- puede tener documentos,
- puede tener firmas conjuntas,
- registra usuario y fecha de finalizacion.

### UsuarioObra y Empleado

El sistema diferencia:

- `UsuarioObra`: identidad para autenticacion, roles y navegacion.
- `Empleado`: ficha laboral y administrativa.

Esta separacion permite modelar operativa de RRHH y acceso al sistema como conceptos relacionados pero no identicos.

### Contrato, fichaje y horario

El modulo de RRHH se apoya en tres entidades:

- `Contrato`: define el marco laboral del trabajador.
- `RegistroFichaje`: registra entrada, salida, estado e incidencia.
- `HorarioAsignado`: representa la planificacion semanal por dia, turno y vigencia.

Esto permite distinguir entre:

- horas previstas,
- horas registradas,
- y restricciones laborales del trabajador.

### Materiales, categorias y proveedores

`Material` se relaciona con:

- una categoria,
- un proveedor principal,
- y varios proveedores adicionales en una relacion muchos a muchos.

Este diseño da soporte tanto a catalogacion interna como a compras.

### SolicitudMaterial

Permite modelar el flujo de peticion y aprobacion:

- solicitante,
- proyecto,
- material,
- revisor,
- estado,
- cantidad,
- fecha y observaciones.

### Factura y Presupuesto

`Presupuesto` mantiene la referencia economica principal del proyecto.

`Factura` soporta:

- proveedor,
- proyecto,
- fechas,
- estado,
- importes base,
- descuento,
- IVA,
- importe total.

## 3. Reglas de modelado con impacto funcional

El modelo no es solo estructural. Varias relaciones sustentan reglas de negocio reales:

- la jerarquia de tareas condiciona cambios de estado,
- las dependencias impiden avances inconsistentes,
- la firma unica por usuario y tarea evita duplicidades,
- el contrato condiciona la capacidad semanal del trabajador,
- la vigencia de horarios impide tratar historicos como asignaciones actuales,
- las relaciones de solicitud y aprobacion permiten trazabilidad de materiales.

## 4. Decisiones de diseño relevantes

### Uso de Identity dentro del mismo contexto

Ventajas:

- simplifica autenticacion y autorizacion,
- evita sincronizacion compleja entre identidad y dominio,
- facilita seed de usuarios demo.

Coste:

- incrementa el acoplamiento del contexto central,
- mezcla persistencia de seguridad con persistencia funcional.

### Separacion entre usuario, empleado y contrato

Esta decision mejora el modelo porque evita que una sola entidad concentre:

- credenciales,
- identidad funcional,
- datos administrativos,
- y condiciones laborales.

### Uso de vigencias en horarios

El uso de `VigenteDesde` y `VigenteHasta` permite tratar horarios como informacion historizable y no solo como un estado sobreescribible. Esta decision es importante para justificar la evolucion del modulo RRHH.

## 5. Lectura academica del modelo

Desde el punto de vista del TFG, el modelo de datos es una de las fortalezas del proyecto porque:

- refleja un dominio realista,
- tiene relaciones no triviales,
- soporta varios modulos coordinados,
- y conecta directamente con reglas funcionales verificables.

No es un esquema de CRUD simple: modela operacion de obra, personas, economia, materiales y trazabilidad documental dentro de un mismo dominio empresarial.
