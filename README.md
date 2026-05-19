# Sistema de Gestión de Obras para PYMEs Constructoras

![Estado](https://img.shields.io/badge/estado-en%20desarrollo-yellow)
![.NET](https://img.shields.io/badge/.NET-10.0-purple)
![Licencia](https://img.shields.io/badge/licencia-MIT-blue)

## 📋 Descripción del Proyecto

Sistema integral de gestión de obras desarrollado como Trabajo de Fin de Grado, diseñado específicamente para modernizar la operativa de pequeñas y medianas empresas (PYMEs) del sector de la construcción en España. 

El aplicativo digitaliza los procesos clave de gestión de proyectos, desde la planificación y control de costes hasta la gestión operativa de tareas, materiales, facturas y recursos humanos. El repositorio también recoge una línea de trabajo de inteligencia normativa basada en **CTE**, **BOE** y normativa autonómica como la **LOTUP**, pero esa parte debe considerarse actualmente **parcial y no cerrada end-to-end** en la aplicación ejecutable.

---

## 🆕 Novedades recientes (13/02/2026)

- **Kanban de tareas mejorado**: subida de documentos en tareas, reglas de jerarquía para movimiento de estados y validación de dependencias entre tareas.
- **Firmas conjuntas reforzadas**: control de firmas duplicadas por usuario/tarea, detalle de pendientes y rechazadas, y bloqueo automático tras rechazo.
- **Modelo de roles ampliado**: interoperabilidad horizontal entre **Jefe de Obra** y **Oficina Técnica**, más soporte de **OperarioObra** y **OperarioOficinaT**.
- **Datos demo realistas**: nuevo seeding de arranque con dataset operativo más completo, activable por configuración (`SeedDemoOnStartup`).
- **Gantt de proyecto rediseñado**: visualización tipo Project, corrección de escala/anchos y scroll horizontal contenido dentro del bloque del diagrama.
- **Gantt agregado de proyectos**: nueva vista en `/proyectos` para ver todos los proyectos en línea temporal, con leyenda por estado y marca visual de fecha de inicio.
- **Navegación corregida**: compatibilidad para ruta de detalle `/proyectos/{id}` evitando errores `Not Found`.

---

## 🎯 Objetivos del Proyecto

### Principales
- **Digitalizar la gestión operativa** eliminando la dependencia del papel y hojas de cálculo desvinculadas
- **Centralizar proyectos, tareas, materiales, costes y personal** en una única aplicación
- **Optimizar el ROI** a través del control en tiempo real de presupuestos, costes y recursos
- **Dejar preparada una base arquitectónica** para futuras integraciones normativas y documentales

### Secundarios
- Facilitar la adopción tecnológica en empresas acogidas al programa **Kit Digital**
- Reducir riesgos jurídicos por uso de normativa obsoleta
- Mejorar la eficiencia en la asignación de recursos humanos y materiales
- Preparar a la empresa para la transición hacia la **Administración Digital 2026**

---

## 🏗️ Arquitectura del Sistema

### Stack Tecnológico

#### Backend
- **ASP.NET Core 10.0** - Framework principal para la API REST y el host web
- **Entity Framework Core** - ORM para gestión de base de datos
- **ASP.NET Core Identity** - Sistema de autenticación y autorización basado en roles (RBAC)

#### Base de Datos
- **Microsoft SQL Server** - Motor de base de datos relacional
- Garantiza integridad referencial y transacciones ACID

#### Frontend
- **Blazor Server** - Framework para UI interactiva
- **Bootstrap 5** - Diseño responsive para acceso multidispositivo
- Optimizado para tablets y móviles en obra

#### Inteligencia Artificial y Servicios Externos
- **OpenAI/Tavily API** - Línea de exploración documental y normativa no cerrada en la app actual
- **Llama 3 + Ollama** - Alternativa conceptual para RAG local, no integrada en el flujo operativo actual
- **Azure Cognitive Services** - Opción de evolución para análisis documental

#### Integración Normativa
- **RSS BOE** - Fuente prevista para vigilancia legislativa futura
- **XML CTE** - Fuente técnica prevista para validaciones futuras
- **PGOU/planeamiento local** - Línea de evolución documental y de consulta, no integrada end-to-end hoy

#### Infraestructura
- **Azure/AWS Cloud** - Hosting en la nube para disponibilidad 24/7
- **HTTPS/TLS** - Cifrado de comunicaciones
- **Docker** - Containerización para despliegue eficiente

---

## 👥 Perfiles de Usuario y Control de Acceso

| Perfil | Permisos | Casos de Uso |
|--------|----------|--------------|
| **Administrador/Gerente** | Acceso total: ROI, aprobación de pagos, configuración global | Supervisión estratégica y financiera |
| **Jefe de Obra** | Gestión de tablero Kanban, planificación y seguimiento de obra | Control operativo a pie de obra |
| **Oficina Técnica** | Apoyo técnico, planificación y seguimiento documental | Coordinación técnica del proyecto |
| **Operario** | Consulta de tareas, tablero personal y fichaje | Registro de jornada y ejecución operativa |
| **RRHH** | Contratos, horarios, validación de fichajes y gestión de personal | Supervisión laboral y administrativa |

---

## ⚙️ Funcionalidades Principales

### 🏛️ Inteligencia Normativa (Alcance parcial)
- **Modelado documental**: base de dominio para carpeta legal y servicios normativos
- **Documentación de arquitectura**: visión de integración con BOE, CTE y normativa autonómica
- **Estado actual**: esta línea no está integrada de forma operativa completa en la aplicación ejecutable

### 📊 Gestión de Proyectos
- **Tablero Kanban Visual**: Organización de fases de obra (Cimentación, Estructura, etc.)
- **Control de Bloqueos**: Justificación técnica obligatoria para tareas paradas
- **Diario de Obra Digital**: Registro cronológico con fotos e incidencias georreferenciadas

### 💰 Control Económico
- **ROI en Tiempo Real**: Dashboard de rentabilidad esperada vs. real
- **Gestión de Facturas**: Vinculación a partidas específicas de obra
- **Comparativa de Presupuestos**: Licitación interna entre proveedores
- **Control de Stock**: Descuento automático del inventario al ejecutar tareas

### 👷 Recursos Humanos y Seguridad
- **Fichaje y validación de jornada**: entrada, salida, histórico y revisión por RRHH
- **Gestión de contratos y horarios**: planificación manual y automática por capacidad operativa
- **PRL**: modelado de cursos y vigencia, pendiente de integración cerrada en todos los flujos operativos
- **Geolocalización**: soporte en entidad de fichaje, con captura de coordenadas aún no cerrada en interfaz

### 📄 Documentación Técnica
- **Exportación a PDF/Excel**: informes y listados desde varios módulos
- **Base para carpeta legal**: modelado parcial pendiente de explotación funcional completa
- **Trazabilidad funcional**: seguimiento de proyectos, tareas, materiales, facturas y RRHH

---

## 📁 Estructura del Repositorio

```
TFG-JORGE/
│
├── src/                              # Código fuente del aplicativo
│   ├── GestionObras.API/             # Proyecto ASP.NET Core Web API
│   ├── GestionObras.Core/            # Capa de dominio y lógica de negocio
│   ├── GestionObras.Infrastructure/  # Acceso a datos, servicios externos
│   └── GestionObras.Web/             # Frontend Blazor
│
├── docs/                             # Documentación técnica del TFG
│   ├── requisitos/                   # Análisis de requisitos funcionales y no funcionales
│   ├── arquitectura/                 # Diagramas de arquitectura y diseño
│   ├── normativa/                    # Integración con BOE, CTE, LOTUP
│   └── manual-usuario/               # Guías de uso por perfil
│
├── tests/                            # Pruebas unitarias e integración
│   ├── GestionObras.Tests/           # Tests del Core
│   └── GestionObras.IntegrationTests/# Tests de API
│
├── scripts/                          # Scripts de automatización
│   ├── database/                     # Migraciones y seeders
│   └── deployment/                   # Scripts de despliegue
│
├── .gitignore                        # Archivos excluidos del control de versiones
├── README.md                         # Este archivo
└── LICENSE                           # Licencia del proyecto
```

---

## 🚀 Requisitos Previos

### Opción recomendada (contenedores)
- **Docker Desktop** o Docker Engine + Compose Plugin
- **Git**

### Opción local (sin contenedores)
- **.NET SDK 10.0 o superior** ([Descargar](https://dotnet.microsoft.com/download))
- **SQL Server 2019+** o **SQL Server Express** ([Descargar](https://www.microsoft.com/sql-server/sql-server-downloads))

### Para Servicios de IA (opcional en desarrollo)
- **Ollama** instalado localmente ([Descargar](https://ollama.ai/))
- Modelo **Llama 3** descargado: `ollama pull llama3`
- API Key de **OpenAI/Tavily** para búsqueda de normativa

---

## 🛠️ Instalación y Configuración

### 1. Clonar el repositorio
```bash
git clone https://github.com/jrg94-ua/tfg-gestion-obras.git
cd tfg-gestion-obras
```

### 2. Arranque recomendado con Docker
```bash
docker compose up -d --build
docker compose ps
```

Servicios y puertos:
- **Web**: `http://localhost:5001`
- **API**: `http://localhost:5000`
- **SQL Server**: `localhost:1433`

Parar entorno:
```bash
docker compose down
```

### 3. Alternativa local con .NET
```bash
cd src
dotnet restore ..\TFG-JORGE.sln
dotnet build ..\TFG-JORGE.sln
```

Para instrucciones completas, ver la guía de instalación actualizada en `docs/INSTALL.md`.

---

## 📊 Casos de Uso Principales

### Caso 1: Inicio de Nuevo Proyecto
1. El **Jefe de Obra** crea un proyecto indicando sus datos principales.
2. El sistema registra la obra, la deja disponible para planificación y seguimiento, y permite asociar tareas, responsables, materiales y presupuestos.
3. Se crea y gestiona el **Tablero Kanban** con las fases y tareas de la obra.

### Caso 2: Gestión de Materiales y Aprovisionamiento
1. La **Oficina Técnica** o el equipo responsable registra materiales y proveedores.
2. El sistema permite crear solicitudes, revisarlas y aprobarlas.
3. La aprobación actualiza stock y deja trazabilidad del aprovisionamiento.

### Caso 3: Monitorización de ROI
1. El **Gerente** accede al dashboard financiero
2. El sistema calcula en tiempo real:
   - Presupuesto inicial vs. costes reales
   - Desviaciones por partidas de obra
   - Proyección de beneficio neto
3. Genera alertas si el margen cae por debajo del umbral configurado

---

## 📝 Requisitos Funcionales Clave

La versión entregable del TFG implementa de forma defendible:

- gestión de proyectos,
- kanban de tareas con dependencias, bloqueos y jerarquía,
- tablero personal por usuario,
- materiales, proveedores y solicitudes,
- presupuestos y facturas,
- contratos, horarios y fichajes,
- exportación documental,
- y autenticación/autorización por roles.

Quedan como líneas parciales o futuras:

- inteligencia normativa automatizada,
- carpeta legal operativa,
- geolocalización completa en fichaje,
- PRL integrada end-to-end en los flujos,
- y notificaciones push.

---

## 🔒 Requisitos No Funcionales

### Seguridad
- **RNF-01**: Control de acceso basado en roles (RBAC)
- **RNF-02**: Integridad referencial de datos
- **RNF-03**: Cifrado HTTPS/TLS en comunicaciones

### Rendimiento
- **RNF-04**: Disponibilidad 24/7 (arquitectura Cloud)
- **RNF-05**: Tiempo de respuesta < 2 segundos
- **RNF-06**: Diseño responsive para móviles y tablets

### Escalabilidad
- **RNF-07**: Arquitectura preparada para Open Data APIs
- **RNF-08**: Soporte de formatos estándar (XML, RSS)
- **RNF-09**: Robustez ante pérdida de conexión

---

## 📚 Documentación Adicional

- [Análisis de Requisitos Completo](docs/requisitos/README.md)
- [Diagramas de Arquitectura](docs/arquitectura/README.md)
- [Integración Normativa y Alcance Real](docs/normativa/README.md)
- [Manual de Usuario por Perfiles](docs/manual-usuario/README.md)
- [Guía de Despliegue](scripts/deployment/README.md)

---

## 🤝 Contribuciones

Este proyecto es un Trabajo de Fin de Grado en desarrollo. Las contribuciones están cerradas hasta la finalización del TFG.

---

## 📄 Licencia

Este proyecto está bajo la Licencia MIT. Ver el archivo [LICENSE](LICENSE) para más detalles.

---

## 👨‍💻 Autor

**Jorge Ros Gómez**  
Trabajo de Fin de Grado - 2026  
Universidad de Alicante (UA)

---

## 📞 Contacto

Para consultas sobre el proyecto:
- Email: [jrg94@gcloud.ua.es]
- LinkedIn: [Tu perfil]

---

## 🙏 Agradecimientos

- Ministerio de Fomento por la documentación del CTE
- Programa Kit Digital del Gobierno de España
- Comunidad de desarrolladores de ASP.NET Core

---

**Nota**: Este proyecto combina un núcleo funcional ya implementado con varias líneas de evolución documentadas. Para la entrega académica conviene defender como implementado el núcleo operativo y presentar la inteligencia normativa, la geolocalización completa y la PRL cerrada como alcance parcial o trabajo futuro.
### Contexto de codigo

- [Rastreo Integral de la Aplicacion](docs/arquitectura/RASTREO_APLICACION.md)
