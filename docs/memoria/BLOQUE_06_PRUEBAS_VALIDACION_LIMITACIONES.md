# Bloque de Memoria: Pruebas, Validacion, Limitaciones y Lineas Futuras

## 1. Introduccion

La validacion constituye una fase esencial en cualquier proyecto de ingenieria del software, ya que permite comprobar que la solucion desarrollada responde a los objetivos planteados y mantiene un comportamiento coherente en sus funciones principales. En el contexto de este TFG, la validacion debe interpretarse desde una perspectiva realista: no solo importa la existencia de pruebas automatizadas, sino tambien la evidencia funcional de que los modulos implementados han sido ejercitados sobre escenarios representativos.

## 2. Estrategia de validacion adoptada

La validacion del sistema se ha apoyado en cuatro mecanismos complementarios:

- comprobacion de compilacion y construccion,
- despliegue reproducible mediante Docker,
- validacion funcional manual de los modulos principales,
- y ejecucion de una suite automatizada sobre reglas de dominio, servicios y persistencia.

Esta combinacion resulta adecuada para un TFG aplicado, donde el objetivo no es un marco industrial completo de calidad, sino una evidencia suficiente y defendible del correcto funcionamiento del sistema.

## 3. Pruebas automatizadas

El proyecto incorpora una suite formal de pruebas en la carpeta `tests`, estructurada en dos proyectos:

- `GestionObras.UnitTests`,
- `GestionObras.IntegrationTests`.

### 3.1 Cobertura actual de la suite

La suite cubre actualmente:

- calculo de ROI en `Proyecto`,
- reglas de firma conjunta en `Tarea`,
- calculo y persistencia de `FacturaService`,
- generacion automatica de horarios en `PlanificacionHorarioService`,
- y comportamiento de `FichajeRepository`.

### 3.2 Ejecucion real de la suite

La ejecucion de la suite se ha validado en contenedor Docker con la imagen oficial `mcr.microsoft.com/dotnet/sdk:10.0`, de forma que la comprobacion no depende del SDK instalado en la maquina anfitriona.

El resultado obtenido ha sido:

- `GestionObras.UnitTests`: 12 pruebas superadas de 12.
- `GestionObras.IntegrationTests`: 3 pruebas superadas de 3.
- Total: 15 pruebas superadas y ninguna fallida.

Este resultado permite afirmar que el proyecto dispone de una base automatizada real y ejecutable, aunque todavia no exhaustiva.

## 4. Validacion funcional y de integracion

Ademas de la suite automatizada, el proyecto cuenta con validacion funcional e integracion realizada a lo largo del desarrollo. Entre las evidencias mas relevantes se encuentran:

- compilaciones y restauraciones satisfactorias,
- despliegue reproducible con Docker,
- arranque conjunto de aplicacion web, API y base de datos,
- validacion del seeding de datos demo,
- pruebas manuales de navegacion por roles,
- comprobaciones funcionales de kanban, RRHH, materiales y gestion economica,
- y validacion de exportaciones PDF y Excel.

Las sesiones documentadas del repositorio sirven como soporte adicional de esta validacion iterativa.

## 5. Presentacion del bloque de pruebas en la memoria

Para una memoria de TFG, la forma mas rigurosa de presentar la validacion consiste en diferenciar tres niveles.

### 5.1 Pruebas tecnicas de construccion

Incluyen:

- restauracion y compilacion del proyecto,
- construccion de imagenes Docker,
- despliegue de servicios,
- y ejecucion de `dotnet test` en contenedor.

### 5.2 Pruebas funcionales manuales

Incluyen la comprobacion guiada de los casos de uso principales:

- autenticacion por roles,
- gestion de proyectos,
- flujo de tareas,
- solicitudes de material,
- gestion de facturas y presupuestos,
- contratos, horarios y fichajes,
- y exportacion documental.

### 5.3 Pruebas automatizadas futuras

Quedan como lineas prioritarias de evolucion:

- ampliar pruebas sobre reglas avanzadas del kanban,
- incorporar pruebas end-to-end de interfaz,
- extender pruebas de integracion a mas repositorios,
- y medir cobertura de forma sistematica.

## 6. Casos de prueba manuales recomendados

Para reforzar la defensa del TFG, resulta recomendable documentar una bateria minima de casos de prueba manuales:

1. inicio de sesion y redireccion segun rol,
2. alta y modificacion de proyecto,
3. flujo de tarea con dependencias y bloqueo,
4. solicitud y aprobacion de material,
5. alta de factura y exportacion,
6. planificacion de horarios con contrato activo,
7. fichaje de entrada, salida y consulta de historico.

Estos casos deben presentarse en tabla con identificador, precondiciones, pasos, resultado esperado, resultado obtenido y evidencia.

## 7. Limitaciones del proceso de pruebas

La principal limitacion del bloque de calidad no es la ausencia de pruebas, sino su alcance todavia parcial. En concreto:

- la validacion manual sigue siendo importante en modulos visuales,
- no existe todavia una bateria end-to-end sobre la interfaz,
- la cobertura automatizada no alcanza la totalidad del sistema,
- y varios componentes documentales y multirol requieren pruebas adicionales.

No obstante, estas limitaciones no impiden afirmar que el proyecto ha sido probado de forma real y reproducible sobre sus modulos principales.

## 8. Lineas futuras de mejora

Las mejoras mas razonables en este ambito serian:

1. ampliar la suite unitaria hacia otros servicios de negocio,
2. extender la integracion automatizada a nuevos repositorios,
3. incorporar pruebas end-to-end de flujos completos,
4. medir cobertura como indicador adicional de calidad,
5. integrar la validacion automatizada en un flujo de construccion continua.

## 9. Conclusion del bloque

La validacion del proyecto se ha sustentado en una combinacion de pruebas automatizadas, despliegue reproducible en contenedores, comprobacion funcional manual y evidencia iterativa recogida durante el desarrollo. Aunque esta estrategia no equivale a una cobertura completa del sistema, si permite afirmar con rigor que la aplicacion ha sido construida, ejecutada y validada sobre sus principales flujos funcionales. Desde el punto de vista academico, esta aproximacion resulta suficiente y defendible para el alcance real del TFG.
