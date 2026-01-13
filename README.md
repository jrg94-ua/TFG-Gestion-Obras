# Sistema de Gestión de Obras para PYMEs Constructoras

![Estado](https://img.shields.io/badge/estado-en%20desarrollo-yellow)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Licencia](https://img.shields.io/badge/licencia-MIT-blue)

## 📋 Descripción del Proyecto

Sistema integral de gestión de obras desarrollado como Trabajo de Fin de Grado, diseñado específicamente para modernizar la operativa de pequeñas y medianas empresas (PYMEs) del sector de la construcción en España. 

El aplicativo digitaliza los procesos clave de gestión de proyectos, desde la planificación y control de costes hasta el cumplimiento normativo automatizado, integrando el **Código Técnico de la Edificación (CTE)**, el **BOE** y normativas autonómicas como la **LOTUP**.

---

## 🎯 Objetivos del Proyecto

### Principales
- **Digitalizar la gestión operativa** eliminando la dependencia del papel y hojas de cálculo desvinculadas
- **Automatizar el cumplimiento normativo** mediante conexión directa con BOE, CTE y planes urbanísticos locales
- **Optimizar el ROI** a través del control en tiempo real de presupuestos, costes y recursos
- **Centralizar la documentación técnica** garantizando una "versión única de la verdad" para todos los actores

### Secundarios
- Facilitar la adopción tecnológica en empresas acogidas al programa **Kit Digital**
- Reducir riesgos jurídicos por uso de normativa obsoleta
- Mejorar la eficiencia en la asignación de recursos humanos y materiales
- Preparar a la empresa para la transición hacia la **Administración Digital 2026**

---

## 🏗️ Arquitectura del Sistema

### Stack Tecnológico

#### Backend
- **ASP.NET Core 8.0** - Framework principal para API RESTful
- **Entity Framework Core** - ORM para gestión de base de datos
- **ASP.NET Core Identity** - Sistema de autenticación y autorización basado en roles (RBAC)

#### Base de Datos
- **Microsoft SQL Server** - Motor de base de datos relacional
- Garantiza integridad referencial y transacciones ACID

#### Frontend
- **Blazor Server/WebAssembly** - Framework para UI interactiva
- **Bootstrap 5** - Diseño responsive para acceso multidispositivo
- Optimizado para tablets y móviles en obra

#### Inteligencia Artificial y Servicios Externos
- **OpenAI/Tavily API** - Capa de descubrimiento de normativa local (PGOU)
- **Llama 3 + Ollama** - Procesamiento local mediante RAG (Retrieval-Augmented Generation)
- **Azure Cognitive Services** - Análisis de documentos técnicos (alternativa)

#### Integración Normativa
- **RSS BOE** - Vigilancia automática de cambios legislativos
- **XML CTE** - Catálogo de Elementos Constructivos del Ministerio
- **Scraping IA** - Extracción de parámetros urbanísticos de PGOU municipales

#### Infraestructura
- **Azure/AWS Cloud** - Hosting en la nube para disponibilidad 24/7
- **HTTPS/TLS** - Cifrado de comunicaciones
- **Docker** - Containerización para despliegue eficiente

---

## 👥 Perfiles de Usuario y Control de Acceso

| Perfil | Permisos | Casos de Uso |
|--------|----------|--------------|
| **Administrador/Gerente** | Acceso total: ROI, aprobación de pagos, configuración global | Supervisión estratégica y financiera |
| **Jefe de Obra** | Gestión de tablero Kanban, mediciones, PRL, documentación | Control operativo a pie de obra |
| **Oficina Técnica** | Vinculación de materiales con CTE, validación técnica | Cumplimiento normativo y certificaciones |
| **Operario** | Fichaje geolocalizado, consulta de tareas, avisos de seguridad | Registro de jornada y consulta básica |

---

## ⚙️ Funcionalidades Principales

### 🏛️ Inteligencia Normativa (Automatizada)
- **Vigilancia BOE**: Notificaciones automáticas de cambios en el CTE
- **Catálogo CTE**: Validación de materiales contra requisitos técnicos oficiales
- **IA Urbanística**: Extracción de parámetros de Planes Generales (PGOU) mediante IA local

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
- **Fichaje Geolocalizado**: Validación de jornada laboral con GPS
- **Validación PRL**: Bloqueo de acceso sin formación de prevención vigente
- **Gestión de Turnos**: Control de horas extra según convenio

### 📄 Documentación Técnica
- **Carpeta Legal Automática**: Generación de repositorio con normativa vigente por proyecto
- **Exportación a PDF**: Actas de obra y certificaciones con formato oficial
- **Trazabilidad Total**: Auditoría completa de cambios y versiones de documentos

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

### Software Necesario
- **.NET SDK 8.0 o superior** ([Descargar](https://dotnet.microsoft.com/download))
- **SQL Server 2019+** o **SQL Server Express** ([Descargar](https://www.microsoft.com/sql-server/sql-server-downloads))
- **Visual Studio 2022** o **VS Code** con extensiones de C#
- **Node.js 18+** (para herramientas de frontend) - Opcional
- **Docker Desktop** (para despliegue containerizado) - Opcional

### Para Servicios de IA (Opcional en desarrollo)
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

### 2. Instalar .NET SDK (si no está instalado)
```bash
# Verificar instalación
dotnet --version

# Si no está instalado, descargar desde:
# https://dotnet.microsoft.com/download/dotnet/8.0
```

### 3. Restaurar dependencias
```bash
cd src
dotnet restore
```

### 4. Configurar la base de datos
```bash
# Editar la cadena de conexión en appsettings.json
# Luego ejecutar las migraciones:
cd GestionObras.API
dotnet ef database update
```

### 5. Ejecutar la aplicación
```bash
dotnet run --project GestionObras.API
```

La API estará disponible en `https://localhost:5001`

---

## 📊 Casos de Uso Principales

### Caso 1: Inicio de Nuevo Proyecto
1. El **Jefe de Obra** crea un proyecto indicando ubicación (Provincia/Municipio) y tipo de suelo
2. El sistema activa automáticamente los **Servicios de Inteligencia**:
   - 🔍 Consulta el BOE para descargar el CTE vigente
   - 🏛️ La IA localiza el PGOU del municipio
   - 📋 Genera la "Carpeta Legal" con toda la normativa aplicable
3. Se crea el **Tablero Kanban** con las fases estándar de obra

### Caso 2: Control de Cumplimiento de Materiales
1. La **Oficina Técnica** añade un material (ej: aislante térmico) al presupuesto
2. El sistema consulta el **Catálogo del CTE** (archivo XML del Ministerio)
3. Valida que el material cumple con la **transmitancia térmica** exigida por el DB-HE
4. Si hay cambios normativos, notifica automáticamente al equipo

### Caso 3: Monitorización de ROI
1. El **Gerente** accede al dashboard financiero
2. El sistema calcula en tiempo real:
   - Presupuesto inicial vs. costes reales
   - Desviaciones por partidas de obra
   - Proyección de beneficio neto
3. Genera alertas si el margen cae por debajo del umbral configurado

---

## 📝 Requisitos Funcionales Clave

### Bloque 1: Inteligencia Normativa
- **RF-01**: Alta de obra con ubicación y tipo de suelo
- **RF-02**: Sincronización automática con BOE (RSS)
- **RF-03**: Integración con Catálogo del CTE (XML)
- **RF-04**: Agente de IA para localización de PGOU
- **RF-05**: Extracción de parámetros urbanísticos (altura, retranqueos, etc.)

### Bloque 2: Operaciones
- **RF-06**: Tablero Kanban de obra con estados (Pendiente, En curso, Bloqueado, Finalizado)
- **RF-07**: Gestión de bloqueos con justificación técnica
- **RF-08**: Registro de mediciones reales vs. presupuestadas
- **RF-09**: Diario de obra digital con fotos y geolocalización

### Bloque 3: Recursos Humanos
- **RF-10**: Asignación de personal a proyectos
- **RF-11**: Fichaje geolocalizado
- **RF-12**: Control de horarios y turnos
- **RF-13**: Validación de formación en PRL

### Bloque 4: Economía
- **RF-14**: Gestión de facturas vinculadas a partidas
- **RF-15**: Control de estados de pago
- **RF-16**: Contabilidad de materiales (descuento automático de stock)
- **RF-17**: Comparativa de presupuestos de proveedores
- **RF-18**: Cálculo de ROI en tiempo real

### Bloque 5: Comunicación
- **RF-19**: Centro de notificaciones push
- **RF-20**: Carpeta documental inteligente por obra

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
- [Integración Normativa](docs/normativa/README.md)
- [Manual de Usuario por Perfiles](docs/manual-usuario/README.md)
- [Guía de Despliegue](docs/deployment.md)

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

**Nota**: Este proyecto está en fase de desarrollo activo como parte de un TFG. La documentación y funcionalidades se actualizan regularmente.
