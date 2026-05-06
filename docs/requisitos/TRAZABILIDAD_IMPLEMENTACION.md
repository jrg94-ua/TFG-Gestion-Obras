# Trazabilidad de Requisitos e Implementacion

## Objetivo

Este documento resume la correspondencia entre los requisitos funcionales mas relevantes del TFG y su estado real de implementacion en la aplicacion.

## Criterio de lectura

Estados utilizados:

- `Implementado`: existe funcionalidad operativa visible y defendible en la aplicacion.
- `Parcial`: existe soporte funcional o de modelado, pero no una solucion completamente cerrada.

## 1. Requisitos funcionales implementados

| ID | Requisito | Estado | Implementacion real |
|----|-----------|--------|---------------------|
| RF-01 | Gestion de proyectos | Implementado | CRUD de proyectos con estado, responsable y filtros |
| RF-02 | Tablero kanban de obra | Implementado | Kanban con columnas, CRUD, asignaciones y cambios de estado |
| RF-03 | Jerarquia de tareas | Implementado | Soporte de subtareas y restricciones de cierre |
| RF-04 | Dependencias entre tareas | Implementado | Predecesoras y bloqueo de transiciones inconsistentes |
| RF-05 | Gestion de bloqueos | Implementado | Bloqueo y desbloqueo con justificacion |
| RF-06 | Asignacion de personal | Implementado | Asignacion multiple y responsable final |
| RF-07 | Tablero personal del trabajador | Implementado | Vista de tareas propias para perfiles operativos |
| RF-08 | Visualizacion temporal del proyecto | Implementado | Vista gantt de seguimiento |
| RF-09 | Gestion de materiales | Implementado | CRUD de materiales, categorias y proveedores |
| RF-10 | Solicitud y aprobacion de material | Implementado | Flujo completo de peticion, revision y aprobacion |
| RF-11 | Gestion de presupuestos | Implementado | CRUD asociado a proyectos |
| RF-12 | Gestion de facturas | Implementado | CRUD, estados y calculo de importes |
| RF-13 | Exportacion documental | Implementado | Exportacion PDF y Excel en varios modulos |
| RF-14 | Gestion de contratos | Implementado | Alta, edicion, consulta y cierre |
| RF-15 | Control de horarios y turnos | Implementado | Horarios por vigencia, gestion manual y consulta |
| RF-16 | Generacion automatica de horarios | Implementado | Planificacion por carga semanal, contrato y perfil |
| RF-17 | Registro de fichajes | Implementado | Entrada, salida, historico y validacion |
| RF-18 | Autenticacion y control de acceso | Implementado | Login, roles, redireccion y navegacion filtrada |
| RF-19 | Administracion de usuarios | Implementado | Alta de usuarios y gestion de roles |

## 2. Requisitos funcionales parciales

| ID | Requisito | Estado | Implementacion real |
|----|-----------|--------|---------------------|
| RF-20 | Control de stock ligado a ejecucion | Parcial | Existe stock y descuento por solicitudes aprobadas, pero no consumo automatico por tarea |
| RF-21 | Fichaje geolocalizado | Parcial | La entidad soporta coordenadas, pero la UI no captura aun la geolocalizacion |
| RF-22 | Carpeta documental del proyecto | Parcial | Existe modelado de `CarpetaLegal`, pero no un modulo completo de explotacion |

## 3. Funcionalidades emergentes con valor real

Durante el desarrollo aparecieron funcionalidades que incrementaron el valor final del sistema y que deben destacarse en la memoria:

- firma conjunta de tareas colaborativas,
- dependencias y jerarquia avanzada de tareas,
- exportacion PDF y Excel en varios modulos,
- tablero personal para operarios,
- modulo completo de RRHH con contratos, horarios y fichajes,
- y planificacion automatica de horarios con restricciones contractuales.

## 4. Interpretacion academica

La mejor forma de defender esta trazabilidad no es insistir en un listado extenso de requisitos no completados, sino mostrar que:

- el sistema cubre con solidez un nucleo funcional amplio,
- los requisitos implementados responden a procesos reales de negocio,
- las extensiones parciales estan claramente delimitadas,
- y el alcance final ha sido gestionado con criterio de priorizacion.

## 5. Conclusion

El proyecto implementa con un grado alto de madurez el nucleo de gestion de proyectos, produccion por tareas, materiales, gestion economica y recursos humanos. Las lineas menos maduras quedan acotadas a un conjunto reducido de extensiones, por lo que no desvirtuan el valor global del sistema ni su defensa como TFG.
