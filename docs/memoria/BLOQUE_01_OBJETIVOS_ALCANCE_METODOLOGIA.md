# Bloque de Memoria: Objetivos, Alcance, Metodologia y Plan de Trabajo

## 1. Introduccion y contexto

El presente Trabajo de Fin de Grado aborda el analisis, diseno e implementacion de una aplicacion web para la gestion integral de obras en pequenas y medianas empresas del sector de la construccion. El problema de partida responde a una situacion habitual en este tipo de organizaciones: coexistencia de procesos fragmentados, uso de hojas de calculo aisladas, escasa trazabilidad entre departamentos y ausencia de una herramienta unificada para coordinar produccion, materiales, documentacion economica y recursos humanos.

Desde esta perspectiva, el proyecto no se limita a informatizar tareas administrativas concretas, sino que persigue estructurar en una unica plataforma varios procesos internos que en la practica suelen gestionarse de forma dispersa. El resultado del trabajo es una aplicacion basada en tecnologias .NET, con acceso por roles, persistencia relacional y un conjunto de modulos funcionales orientados a distintos perfiles de la organizacion.

## 2. Objetivo general

El objetivo general del proyecto es disenar e implementar un sistema de informacion para la gestion integral de obras que permita centralizar la operativa principal de una empresa constructora, mejorar la coordinacion entre perfiles profesionales y aumentar la trazabilidad de la informacion durante el ciclo de vida del proyecto.

## 3. Objetivos especificos

Para alcanzar el objetivo general se establecieron los siguientes objetivos especificos:

1. Analizar el dominio de gestion de obras e identificar los procesos mas relevantes para su digitalizacion.
2. Definir un modelo de datos coherente con la estructura real de proyectos, tareas, usuarios, materiales, facturas y recursos humanos.
3. Construir una arquitectura modular que separe dominio, persistencia, presentacion y servicios de negocio.
4. Implementar un modulo de gestion de proyectos y tareas con soporte para estados, responsables, subtareas, dependencias y bloqueos.
5. Incorporar un bloque de gestion de materiales, proveedores y solicitudes internas de aprovisionamiento.
6. Implementar funcionalidades de gestion economica mediante presupuestos, facturas y exportacion documental.
7. Desarrollar un modulo de recursos humanos con contratos, horarios y registro de fichajes.
8. Integrar autenticacion y autorizacion por roles para adaptar la aplicacion al perfil de cada usuario.
9. Generar una documentacion tecnica suficiente para justificar el trabajo desde una perspectiva academica y de ingenieria del software.

## 4. Alcance del proyecto

El alcance del TFG se ha orientado a la construccion de un sistema funcional y demostrable, centrado en el nucleo de gestion interna de una empresa de construccion. La prioridad no ha sido incorporar todas las lineas conceptuales planteadas en fases tempranas, sino consolidar un conjunto de modulos con valor operativo real.

### 4.1 Alcance funcional implementado

Dentro del alcance efectivamente desarrollado se incluyen:

- autenticacion y control de acceso por roles,
- gestion de proyectos con responsables y estados,
- gestion de tareas mediante kanban,
- soporte de subtareas, dependencias, bloqueos y firmas conjuntas,
- tablero personal para perfiles operativos,
- visualizacion temporal tipo gantt,
- gestion de usuarios y empleados,
- gestion de materiales, categorias y proveedores,
- solicitudes de material con flujo de aprobacion,
- gestion de presupuestos y facturas,
- exportacion PDF y Excel en varios modulos,
- gestion de contratos, horarios y fichajes,
- generacion automatica de horarios basada en capacidad operativa,
- y despliegue reproducible mediante Docker.

### 4.2 Alcance reservado como mejora futura

Se han dejado fuera del cierre principal algunas lineas que, aunque aportaban interes conceptual, no resultaban criticas para consolidar el nucleo funcional del sistema. Entre ellas se encuentran:

- integraciones normativas automatizadas con fuentes externas,
- analisis territorial avanzado,
- notificaciones push,
- diario de obra como modulo independiente,
- y una API de negocio plenamente desacoplada.

Esta delimitacion no responde a una carencia accidental, sino a una decision de alcance orientada a priorizar un resultado robusto y defendible.

## 5. Metodologia de desarrollo

El desarrollo del proyecto ha seguido una metodologia iterativa e incremental. En lugar de partir de una especificacion completamente cerrada y avanzar de manera lineal, se ha optado por ciclos sucesivos de analisis, implementacion, validacion y refactorizacion. Este enfoque ha permitido adaptar el sistema al conocimiento progresivo del dominio y a la deteccion de necesidades reales durante la construccion.

La metodologia aplicada puede aproximarse a un marco agil de pequeno alcance, apoyado en backlog funcional, sesiones de trabajo documentadas y validacion continua de los modulos desarrollados.

### 5.1 Principios metodologicos aplicados

Durante el desarrollo se han seguido varios principios:

- priorizar modulos con valor funcional demostrable,
- completar flujos operativos antes de abrir nuevas lineas de trabajo,
- separar progresivamente responsabilidades entre interfaz, servicios y persistencia,
- validar iterativamente el comportamiento del sistema,
- y documentar decisiones tecnicas y cambios de alcance.

### 5.2 Papel de las sesiones documentadas

Las sesiones recogidas en `docs/sesiones` constituyen una evidencia relevante del proceso seguido. Su utilidad principal ha sido:

- registrar decisiones de implementacion,
- dejar constancia de problemas tecnicos y correcciones,
- justificar la evolucion del alcance,
- y aportar trazabilidad entre el analisis inicial y el estado final del sistema.

## 6. Plan de trabajo

Aunque el desarrollo ha sido iterativo, puede sintetizarse en las siguientes fases:

### Fase 1. Analisis del problema y definicion inicial

Se identificaron los actores del sistema, los procesos a digitalizar y una primera especificacion de requisitos funcionales y no funcionales.

### Fase 2. Diseno de arquitectura y modelo de dominio

Se definio la estructura de proyectos de la solucion, el modelo de entidades principal y las relaciones necesarias para sostener la aplicacion.

### Fase 3. Implementacion del nucleo funcional

Se desarrollaron los modulos base de autenticacion, proyectos y gestion de tareas, estableciendo el flujo operativo central de la aplicacion.

### Fase 4. Enriquecimiento de reglas de negocio

Se incorporaron dependencias entre tareas, jerarquia padre-subtarea, firmas conjuntas, bloqueos y gestion documental asociada.

### Fase 5. Incorporacion de modulos empresariales de soporte

Se desarrollaron los bloques de materiales, presupuestos, facturas y recursos humanos, ampliando el sistema hacia una gestion mas integral.

### Fase 6. Consolidacion tecnica y validacion

Se reforzaron CRUD reales, se extrajo logica a servicios, se introdujeron exportaciones documentales y se implemento una suite formal de pruebas automatizadas.

### Fase 7. Cierre documental

La fase final se ha orientado a convertir el repositorio en un TFG defendible, con bloques de memoria, trazabilidad, pruebas, arquitectura y limitaciones expresadas de forma explicita.

## 7. Justificacion del enfoque adoptado

Desde una perspectiva de ingenieria del software, el enfoque seguido resulta adecuado para un TFG por varias razones:

- permite entregar un sistema funcional y verificable,
- facilita la deteccion temprana de inconsistencias del modelo,
- favorece una gestion realista del alcance,
- y deja evidencia suficiente del proceso de analisis, construccion y validacion.

Ademas, el enfoque iterativo refleja una practica profesional verosimil: la solucion no emerge de una sola especificacion cerrada, sino de un refinamiento progresivo del sistema hasta alcanzar un nivel razonable de madurez funcional.

## 8. Conclusion del bloque

En conjunto, el proyecto puede definirse como el desarrollo de una aplicacion de gestion integral de obras construida con un criterio pragmatico, incremental y orientado a resultado. La combinacion de objetivos claros, alcance bien delimitado y metodologia iterativa permite presentar el trabajo como una solucion tecnicamente coherente y academicamente defendible dentro del marco de un TFG de Ingenieria Informatica.
