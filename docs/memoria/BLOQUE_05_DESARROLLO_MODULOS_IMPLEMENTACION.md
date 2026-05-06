# Bloque de Memoria: Desarrollo e Implementacion de Modulos Funcionales

## 1. Introduccion

Una vez definidos los objetivos, la arquitectura, el modelo de datos y la trazabilidad de requisitos, resulta necesario describir como se ha materializado la solucion en forma de modulos funcionales. Este bloque expone la implementacion de las principales areas del sistema y explica su aportacion al problema planteado.

Desde la perspectiva del TFG, este apartado es especialmente relevante porque conecta el analisis previo con el resultado tangible del proyecto.

## 2. Vision general de la implementacion

La aplicacion no se ha construido como un conjunto de pantallas independientes, sino como una solucion modular sobre un modelo de datos comun, seguridad por roles y persistencia centralizada. Los modulos principales del sistema son:

- autenticacion y perfiles,
- proyectos,
- planificacion y control de tareas,
- materiales y aprovisionamiento,
- gestion economica,
- recursos humanos,
- y exportacion documental.

## 3. Modulo de autenticacion, autorizacion y perfiles

Este modulo controla el acceso al sistema y adapta la experiencia de uso segun el perfil autenticado. Su implementacion se apoya en `ASP.NET Core Identity` e incluye:

- inicio y cierre de sesion,
- alta de usuarios,
- asignacion de roles,
- redireccion por perfil,
- y filtrado de navegacion.

Su valor funcional es doble: protege el sistema y simplifica la experiencia del usuario al mostrar solo las funciones que le corresponden.

## 4. Modulo de gestion de proyectos

El modulo de proyectos constituye la puerta de entrada a la operativa del sistema. Permite:

- alta, edicion y eliminacion de proyectos,
- asignacion de responsable,
- filtrado y busqueda,
- consulta por estados,
- y acceso a vistas relacionadas como kanban, gantt e historial de materiales.

Desde el punto de vista del dominio, el proyecto actua como contenedor principal del resto de la informacion operativa.

## 5. Modulo de planificacion y control de tareas

Se trata del bloque funcional mas desarrollado del sistema. Su implementacion gira en torno al kanban del proyecto y al tablero personal del trabajador.

### 5.1 Kanban de proyecto

El kanban permite:

- crear, editar y eliminar tareas,
- crear subtareas,
- mover tareas entre estados,
- asignar varios usuarios,
- designar un responsable final,
- definir dependencias,
- bloquear y desbloquear tareas,
- adjuntar documentos,
- registrar observaciones de cierre,
- y exportar a PDF y Excel.

### 5.2 Reglas de negocio del modulo

Sobre esta interfaz se han implementado reglas como:

- imposibilidad de avanzar tareas con predecesoras no finalizadas,
- imposibilidad de cerrar tareas padre con subtareas abiertas,
- control de coherencia entre tareas y subtareas,
- y firma conjunta para tareas colaborativas.

### 5.3 Tablero personal

Como complemento al kanban general, la aplicacion incorpora un tablero personal orientado a perfiles operativos. Esta vista simplifica la experiencia y concentra la interaccion en el trabajo asignado al usuario.

### 5.4 Vista gantt

La planificacion se completa con una vista tipo gantt orientada a seguimiento temporal, supervision y consulta visual del proyecto.

## 6. Modulo de materiales, proveedores y solicitudes

Este modulo se ha desarrollado para integrar aprovisionamiento y produccion dentro de la misma plataforma. Incluye:

- CRUD de materiales,
- gestion de categorias,
- gestion de proveedores,
- control basico de stock,
- flujo de solicitud de materiales,
- y revision, aprobacion o rechazo de solicitudes.

Su aportacion principal es evitar que la gestion de materiales quede fuera del sistema operativo de la obra.

## 7. Modulo de gestion economica

La gestion economica se articula sobre dos entidades principales: presupuestos y facturas.

### 7.1 Presupuestos

El sistema permite registrar y mantener presupuestos asociados a proyectos, proporcionando una referencia economica sobre la que apoyar el seguimiento posterior.

### 7.2 Facturas

El modulo de facturas soporta:

- alta y edicion,
- asociacion con proveedor y proyecto,
- gestion de estados,
- y calculo de base imponible, descuento, IVA e importe total.

Parte de esta logica se ha desplazado a servicios de aplicacion para mejorar coherencia y reutilizacion.

### 7.3 Exportacion economica

Los modulos economicos incorporan exportacion documental, reforzando su utilidad practica y el valor demostrable del sistema.

## 8. Modulo de recursos humanos

El bloque de recursos humanos es uno de los modulos con mayor crecimiento y consolidacion durante el desarrollo.

### 8.1 Contratos

Permite registrar informacion contractual como tipo de contrato, jornada, horas semanales, vigencia y datos administrativos complementarios.

### 8.2 Horarios

La gestion de horarios permite:

- asignar turnos,
- consultar vigencias,
- editar planificaciones,
- y generar horarios automaticamente por proyecto.

La generacion automatica incorpora restricciones ligadas a contrato, limite diario y perfil operativo.

### 8.3 Fichajes

El sistema registra fichajes de entrada y salida, mantiene historico y permite validacion desde perfiles de supervision y recursos humanos.

### 8.4 Valor del modulo

La inclusion de este bloque refuerza de manera notable el alcance del TFG, al ampliar la aplicacion desde la gestion de tareas hacia una gestion interna mas completa del personal.

## 9. Modulo de exportacion documental

La aplicacion incorpora servicios de exportacion basados en `QuestPDF` y `ClosedXML`. Estas exportaciones se aplican a distintos conjuntos de datos, entre ellos:

- facturas,
- presupuestos,
- empleados,
- contratos,
- horarios,
- fichajes,
- y tareas.

Este bloque aporta un valor especialmente visible en la demostracion del sistema, ya que transforma informacion operativa en documentos reutilizables.

## 10. Dashboards y vistas resumidas

La aplicacion incorpora paneles diferenciados para administracion, jefatura de obra, oficina tecnica y recursos humanos. Su finalidad es mostrar:

- metricas agregadas,
- accesos rapidos,
- tablas de resumen,
- y alertas operativas.

Estos paneles mejoran la usabilidad y refuerzan la adaptacion por roles.

## 11. Integracion entre modulos

Uno de los aspectos mas relevantes del desarrollo es la integracion interna entre modulos:

- los proyectos contienen tareas,
- las tareas se relacionan con usuarios,
- las solicitudes de material se vinculan a proyectos,
- los contratos condicionan la planificacion de horarios,
- los fichajes reflejan actividad real sobre proyectos,
- y los documentos economicos completan la trazabilidad de la obra.

Esta integracion es uno de los elementos que permiten defender la solucion como un sistema de gestion integral y no como una coleccion de herramientas aisladas.

## 12. Valor global del conjunto implementado

Considerado en su conjunto, el sistema cubre varias dimensiones del problema empresarial:

- produccion,
- control de tareas,
- aprovisionamiento,
- gestion economica,
- y recursos humanos.

La amplitud funcional, combinada con reglas de negocio reales, constituye una de las principales fortalezas del proyecto desde el punto de vista academico.

## 13. Conclusion del bloque

La implementacion de modulos funcionales muestra una construccion progresiva pero coherente de una aplicacion empresarial compleja. Cada modulo responde a una necesidad concreta del dominio y, al mismo tiempo, se integra con el resto del sistema para formar una solucion unificada, util y tecnicamente defendible en el marco del TFG.
