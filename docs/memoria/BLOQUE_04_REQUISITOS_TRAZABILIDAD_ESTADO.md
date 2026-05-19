# Bloque de Memoria: Requisitos del Sistema, Trazabilidad y Estado de Implementacion

## 1. Introduccion

La definicion de requisitos constituye un elemento esencial en cualquier proyecto de ingenieria del software, ya que establece el marco funcional sobre el que se construye la solucion. En este TFG, los requisitos se formularon a partir del problema detectado en la gestion de obras de pequenas y medianas empresas constructoras, y fueron refinados a medida que avanzaba el desarrollo.

Desde una perspectiva academica, no resulta suficiente enumerar requisitos: es necesario relacionarlos con la implementacion real y explicar de forma honesta el grado de cumplimiento alcanzado. Por ello, este bloque se centra en los requisitos funcionales efectivamente defendibles a partir del estado actual de la aplicacion.

Como complemento a este bloque, el documento `ANEXO_RF_FLUJOS_IMPLEMENTACION.md` recoge para cada requisito implementado el flujo tecnico, los archivos clave y un fragmento de codigo representativo, con el fin de facilitar la defensa oral del proyecto.

## 2. Requisitos funcionales implementados

La Tabla 1 resume los requisitos funcionales principales que pueden considerarse implementados con un grado suficiente de madurez.

| ID | Requisito funcional | Estado | Evidencia funcional |
|----|---------------------|--------|---------------------|
| RF-01 | Gestion de proyectos | Implementado | Alta, edicion, consulta, cambio de estado y asignacion de responsable |
| RF-02 | Gestion visual de tareas en obra | Implementado | Kanban de proyecto con estados, CRUD y drag and drop |
| RF-03 | Gestion de subtareas y jerarquia | Implementado | Relacion padre-subtarea y restricciones de cierre |
| RF-04 | Control de dependencias entre tareas | Implementado | Predecesoras y bloqueo de transiciones inconsistentes |
| RF-05 | Gestion de bloqueos de produccion | Implementado | Bloqueo y desbloqueo con justificacion tecnica |
| RF-06 | Asignacion de personal a tareas | Implementado | Usuarios asignados y responsable final |
| RF-07 | Tablero personal del trabajador | Implementado | Vista simplificada de tareas propias por usuario |
| RF-08 | Visualizacion temporal del proyecto | Implementado | Vista gantt de seguimiento |
| RF-09 | Gestion de materiales y proveedores | Implementado | CRUD de materiales, categorias, proveedor principal y proveedores asociados |
| RF-10 | Solicitud y aprobacion de materiales | Implementado | Flujo de peticion, revision, aprobacion o rechazo |
| RF-11 | Gestion de presupuestos | Implementado | CRUD asociado a proyectos |
| RF-12 | Gestion de facturas y gastos | Implementado | CRUD, estados, calculo de importes y relacion con proveedor y proyecto |
| RF-13 | Exportacion documental | Implementado | Exportacion PDF y Excel en varios modulos |
| RF-14 | Gestion de contratos | Implementado | Alta, edicion, finalizacion y consulta |
| RF-15 | Gestion de horarios | Implementado | Alta, edicion, vigencias y consulta semanal |
| RF-16 | Generacion automatica de horarios | Implementado | Planificacion basada en carga, contrato y perfil operativo |
| RF-17 | Registro de fichajes | Implementado | Entrada, salida, historico y validacion por RRHH |
| RF-18 | Autenticacion y autorizacion por roles | Implementado | Login, redireccion por rol y filtrado de navegacion |
| RF-19 | Administracion de usuarios y perfiles | Implementado | Alta de usuarios y gestion de roles |

## 3. Requisitos funcionales implementados parcialmente

Algunos requisitos cuentan con una base funcional o de modelado suficiente para ser mencionados, aunque no deben presentarse como completamente cerrados:

| ID | Requisito funcional | Estado | Alcance actual |
|----|---------------------|--------|----------------|
| RF-20 | Control de stock ligado a operativa de obra | Parcial | Existe stock y descuento por aprobacion de solicitudes, pero no consumo automatico por ejecucion de tarea |
| RF-21 | Registro de jornada con geolocalizacion | Parcial | La entidad soporta coordenadas, pero la captura en interfaz no esta cerrada |
| RF-22 | Carpeta documental del proyecto | Parcial | Existe modelado de `CarpetaLegal`, pero no un modulo completo de explotacion funcional |

Ademas, existen dos lineas de alcance que deben formularse con honestidad tecnica:

- la validacion PRL esta modelada en dominio, pero no integrada de forma cerrada y uniforme en todos los flujos operativos,
- y la inteligencia normativa automatizada forma parte de la vision ampliada del TFG, no del nucleo funcional completamente implementado.

## 4. Requisitos no incluidos en el cierre principal

Con el fin de no sobredimensionar el apartado de limitaciones, solo se recogen aqui aquellas lineas que realmente pertenecian a una vision mas amplia del proyecto y que quedaron fuera del cierre principal:

- automatizacion normativa avanzada,
- inteligencia territorial sobre PGOU,
- modulo diferenciado de diario de obra,
- notificaciones push.

Estas lineas no deben ocupar el centro del discurso de la memoria, ya que no forman parte del nucleo funcional que estructura la aplicacion final.

## 5. Requisitos no funcionales

Ademas de los requisitos funcionales, el sistema responde a varios requisitos no funcionales relevantes:

### 5.1 Seguridad

La aplicacion incorpora autenticacion, autorizacion por roles y control de acceso segmentado segun perfil de usuario. No obstante, la defensa debe distinguir entre esta capa de seguridad ya operativa y las validaciones especificas de PRL, que todavia no condicionan todos los flujos de trabajo de extremo a extremo.

### 5.2 Integridad y consistencia

El uso de base de datos relacional y `Entity Framework Core` permite mantener coherencia entre proyectos, tareas, contratos, fichajes, materiales y documentos economicos.

### 5.3 Mantenibilidad

Aunque la arquitectura no separa todas las responsabilidades en proyectos independientes, se han introducido servicios y repositorios para reducir acoplamiento y mejorar la evolucion del sistema.

### 5.4 Usabilidad por roles

La interfaz se ha organizado para adaptarse a distintos perfiles profesionales, simplificando la experiencia en usuarios operativos y concentrando funciones avanzadas en perfiles administrativos o de supervision.

## 6. Trazabilidad entre requisitos e implementacion

La trazabilidad permite comprobar que el resultado final no es una acumulacion arbitraria de pantallas, sino una solucion alineada con los objetivos funcionales del proyecto. En este sentido, puede afirmarse que el nucleo del sistema se articula sobre cuatro ejes claramente implementados:

- gestion de proyectos y produccion,
- aprovisionamiento y materiales,
- gestion economica,
- y recursos humanos.

La relacion entre requisitos y modulos reales del sistema es directa:

- los requisitos RF-01 a RF-08 se materializan principalmente en proyectos, kanban, tablero personal y gantt,
- los requisitos RF-09 y RF-10 se cubren en materiales, catalogos y solicitudes,
- los requisitos RF-11 a RF-13 se materializan en presupuestos, facturas y exportacion documental,
- y los requisitos RF-14 a RF-19 se reflejan en contratos, horarios, fichajes y control de acceso.

## 7. Evolucion del alcance

Durante el desarrollo aparecieron funcionalidades que, aunque no siempre figuraban con ese nivel de detalle en la formulacion inicial, han terminado aportando un valor muy significativo al sistema. Entre ellas destacan:

- firma conjunta de tareas colaborativas,
- dependencias entre tareas,
- jerarquia padre-subtarea,
- exportaciones PDF y Excel en distintos modulos,
- tablero personal para operarios,
- y generacion automatica de horarios con restricciones contractuales.

Estas capacidades deben presentarse en la memoria como una evolucion razonable del alcance y no como una desviacion respecto a los objetivos del TFG.

## 8. Estado global de implementacion

Desde una vision agregada, el proyecto presenta un grado alto de implementacion en sus modulos centrales. Las areas mas consolidadas son:

- gestion de proyectos,
- gestion de tareas y reglas del kanban,
- materiales y aprovisionamiento,
- gestion economica basica,
- contratos, horarios y fichajes,
- y seguridad por roles.

Las lineas menos maduras quedan acotadas a un conjunto reducido de extensiones futuras, especialmente normativa automatizada, PRL operativa cerrada y geolocalizacion completa, y no deben desdibujar el peso de lo que si esta resuelto.

## 9. Valor academico de la trazabilidad

La trazabilidad aporta valor academico porque demuestra:

- comprension del problema,
- capacidad para priorizar,
- implementacion de una parte sustancial del sistema,
- y claridad al distinguir entre funcionalidad consolidada y trabajo futuro.

Esta aproximacion resulta mas rigurosa que afirmar un cumplimiento total no realista o, en el extremo contrario, sobredimensionar las partes no implementadas.

## 10. Conclusion del bloque

El analisis de requisitos y su correspondencia con la implementacion permite concluir que el proyecto ha alcanzado un nivel elevado de realizacion en el nucleo funcional de la aplicacion. La solucion final cubre con solidez gestion de proyectos, produccion por tareas, aprovisionamiento, gestion economica y recursos humanos, que son precisamente las areas que aportan mayor valor tecnico y funcional al TFG.
