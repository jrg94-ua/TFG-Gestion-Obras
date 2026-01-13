# 🚀 Sprint 1: Fundamentos y Arquitectura Base

**Duración**: 2 semanas (10 días laborables)  
**Fecha Inicio**: 13 de enero de 2026  
**Fecha Fin**: 27 de enero de 2026

---

## 🎯 Objetivos del Sprint

El Sprint 1 establece los cimientos técnicos del sistema de gestión de obras. Al finalizar este sprint, tendremos una aplicación funcional con:

1. ✅ **Base de datos funcional** con todas las entidades principales
2. ✅ **Sistema de autenticación** con 4 roles diferenciados
3. ✅ **Dashboard operativo** con métricas básicas
4. ✅ **Gestión completa de proyectos** (CRUD)
5. ✅ **Tablero Kanban básico** para gestión de tareas

---

## 📋 Product Backlog del Sprint

### **HISTORIA DE USUARIO 1**: Como gerente, necesito que el sistema almacene mis proyectos de forma persistente

**Criterios de Aceptación**:
- [ ] La base de datos SQL Server está creada y funcional
- [ ] Puedo crear un proyecto y verlo al recargar la página
- [ ] Los datos de proyectos, tareas y empleados se relacionan correctamente

**Tareas Técnicas**:
1. **[TAREA 1.1]** Configurar ApplicationDbContext y modelos
   - **Descripción**: Crear `ApplicationDbContext.cs` en `GestionObras.Infrastructure/Data/`
   - **DbSets necesarios**:
     ```csharp
     DbSet<Proyecto> Proyectos
     DbSet<Tarea> Tareas
     DbSet<Empleado> Empleados
     DbSet<Material> Materiales
     DbSet<Factura> Facturas
     DbSet<Presupuesto> Presupuestos
     DbSet<CarpetaLegal> CarpetasLegales
     DbSet<CursoPRL> CursosPRL
     DbSet<Fichaje> Fichajes
     DbSet<Proveedor> Proveedores
     ```
   - **Configuraciones de Fluent API**:
     - Relaciones uno-a-muchos (Proyecto → Tareas)
     - Relaciones muchos-a-muchos (Empleado ↔ Proyecto)
     - Índices para búsquedas rápidas (DNI, Código Material)
     - Constraints de integridad referencial
   - **Estimación**: 4 horas
   - **Prioridad**: CRÍTICA

2. **[TAREA 1.2]** Configurar cadena de conexión SQL Server
   - **Descripción**: Editar `appsettings.json` en ambos proyectos (API y Web)
   - **Connection String**:
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GestionObrasDB;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
     ```
   - **Registrar en Program.cs**:
     ```csharp
     builder.Services.AddDbContext<ApplicationDbContext>(options =>
         options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
     ```
   - **Estimación**: 1 hora
   - **Prioridad**: CRÍTICA

3. **[TAREA 1.3]** Crear y aplicar migración inicial
   - **Descripción**: Generar la estructura de base de datos
   - **Comandos**:
     ```bash
     cd GestionObras.Infrastructure
     dotnet ef migrations add InitialCreate --startup-project ../GestionObras.Web
     dotnet ef database update --startup-project ../GestionObras.Web
     ```
   - **Verificación**: Abrir SQL Server Object Explorer en VS Code y confirmar que las tablas se crearon
   - **Estimación**: 2 horas (incluye resolución de errores)
   - **Prioridad**: CRÍTICA

---

### **HISTORIA DE USUARIO 2**: Como administrador, necesito controlar quién accede al sistema según su rol

**Criterios de Aceptación**:
- [ ] Puedo registrar usuarios con roles específicos
- [ ] Los usuarios solo ven las secciones permitidas según su rol
- [ ] El sistema protege rutas no autorizadas con redirección a Login

**Tareas Técnicas**:
4. **[TAREA 2.1]** Configurar ASP.NET Core Identity con roles
   - **Descripción**: Extender IdentityUser y crear ApplicationUser
   - **Archivo**: `GestionObras.Infrastructure/Identity/ApplicationUser.cs`
   - **Roles a crear**:
     ```csharp
     public static class Roles
     {
         public const string Administrador = "Administrador";
         public const string JefeObra = "JefeObra";
         public const string OficinaTecnica = "OficinaTecnica";
         public const string Operario = "Operario";
     }
     ```
   - **Configuración en Program.cs**:
     ```csharp
     builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
         options.Password.RequireDigit = true;
         options.Password.RequiredLength = 6;
     })
     .AddEntityFrameworkStores<ApplicationDbContext>()
     .AddDefaultTokenProviders();
     ```
   - **Seed de roles**: Crear DataSeeder.cs para insertar los 4 roles al inicio
   - **Estimación**: 3 horas
   - **Prioridad**: ALTA

5. **[TAREA 2.2]** Crear páginas de Login y Registro
   - **Archivos**:
     - `GestionObras.Web/Components/Pages/Auth/Login.razor`
     - `GestionObras.Web/Components/Pages/Auth/Register.razor`
   - **Funcionalidades Login**:
     - Formulario con Email + Password
     - Validación del lado cliente (DataAnnotations)
     - Redirección a Dashboard tras login exitoso
   - **Funcionalidades Registro**:
     - Formulario con: Email, Password, Confirm Password, Nombre, DNI
     - Dropdown para seleccionar rol (solo visible para Administrador)
     - Validación de DNI español (regex)
   - **Configurar AuthenticationStateProvider** en Blazor
   - **Estimación**: 5 horas
   - **Prioridad**: ALTA

---

### **HISTORIA DE USUARIO 3**: Como jefe de obra, necesito ver un panel de control con el estado general de mis proyectos

**Criterios de Aceptación**:
- [ ] Al hacer login, veo un dashboard con tarjetas de resumen
- [ ] El dashboard muestra: total de proyectos, tareas pendientes, proyectos bloqueados
- [ ] Puedo navegar desde el dashboard a las secciones de Proyectos y Kanban

**Tareas Técnicas**:
6. **[TAREA 3.1]** Implementar Dashboard principal
   - **Archivo**: `GestionObras.Web/Components/Pages/Dashboard.razor`
   - **Componentes visuales** (usar Bootstrap Cards):
     ```
     ┌─────────────┬─────────────┬─────────────┐
     │  Proyectos  │   Tareas    │    ROI      │
     │   Activos   │  Pendientes │   Promedio  │
     │     🏗️ 5    │    ⏳ 23    │   📈 12.5%  │
     └─────────────┴─────────────┴─────────────┘
     
     ┌─────────────────────────────────────────┐
     │  ⚠️ Alertas y Notificaciones            │
     │  • Proyecto "Casa Valencia" bloqueado   │
     │  • 3 empleados sin PRL vigente          │
     └─────────────────────────────────────────┘
     
     ┌─────────────────────────────────────────┐
     │  📊 Proyectos Recientes                 │
     │  [Tabla con últimos 5 proyectos]        │
     └─────────────────────────────────────────┘
     ```
   - **Datos a mostrar**:
     - Total de proyectos (por estado: Planificación, EnCurso, Bloqueado)
     - Total de tareas (por estado Kanban)
     - ROI promedio calculado con `Proyecto.CalcularROIActual()`
     - Lista de alertas (proyectos bloqueados, empleados sin PRL)
   - **Responsive**: Adaptar a móvil (1 columna) y desktop (3 columnas)
   - **Estimación**: 4 horas
   - **Prioridad**: ALTA

---

### **HISTORIA DE USUARIO 4**: Como gerente, necesito crear, editar y eliminar proyectos de construcción

**Criterios de Aceptación**:
- [ ] Puedo ver una lista de todos mis proyectos
- [ ] Puedo crear un proyecto con ubicación, tipo de suelo y fechas
- [ ] Puedo editar un proyecto existente
- [ ] Puedo eliminar un proyecto (con confirmación)

**Tareas Técnicas**:
7. **[TAREA 4.1]** Crear módulo de Proyectos (CRUD)
   - **Archivo**: `GestionObras.Web/Components/Pages/Proyectos.razor`
   - **Vista de Lista**:
     - Tabla con columnas: Nombre, Ubicación, Estado, Fecha Inicio, ROI, Acciones
     - Botón "Nuevo Proyecto" que abre modal
     - Filtros: Por estado, por ubicación
     - Paginación (10 proyectos por página)
   - **Formulario de Creación/Edición**:
     ```
     ┌─────────────────────────────────────┐
     │  Nuevo Proyecto                     │
     ├─────────────────────────────────────┤
     │  Nombre: [____________________]     │
     │  Provincia: [Dropdown ▼]            │
     │  Municipio: [____________________]  │
     │  Tipo Suelo: ( ) Urbano (•) Rústico│
     │  Fecha Inicio: [📅 DD/MM/AAAA]      │
     │  Presupuesto: [______] €            │
     │                                     │
     │  [Cancelar]  [Guardar Proyecto]     │
     └─────────────────────────────────────┘
     ```
   - **Validaciones**:
     - Nombre obligatorio (3-100 caracteres)
     - Ubicación obligatoria
     - Presupuesto > 0
     - Fecha inicio < Fecha fin estimada
   - **Estimación**: 6 horas
   - **Prioridad**: CRÍTICA

8. **[TAREA 4.2]** Implementar repositorios básicos
   - **Archivos a crear**:
     - `GestionObras.Infrastructure/Repositories/ProyectoRepository.cs`
     - `GestionObras.Infrastructure/Repositories/TareaRepository.cs`
     - `GestionObras.Infrastructure/Repositories/EmpleadoRepository.cs`
   - **Métodos básicos** (implementar IRepository<T>):
     ```csharp
     Task<List<T>> GetAllAsync();
     Task<T?> GetByIdAsync(int id);
     Task<T> AddAsync(T entity);
     Task UpdateAsync(T entity);
     Task DeleteAsync(int id);
     ```
   - **Métodos específicos ProyectoRepository**:
     ```csharp
     Task<List<Proyecto>> GetProyectosByEstadoAsync(EstadoProyecto estado);
     Task<List<Proyecto>> GetProyectosByUbicacionAsync(string provincia);
     Task<decimal> GetROIPromedioAsync();
     ```
   - **Inyección de dependencias**: Registrar en Program.cs
   - **Estimación**: 4 horas
   - **Prioridad**: CRÍTICA

---

### **HISTORIA DE USUARIO 5**: Como jefe de obra, necesito organizar las tareas en un tablero visual tipo Kanban

**Criterios de Aceptación**:
- [ ] Veo un tablero con 4 columnas: Pendiente, En Curso, Bloqueado, Finalizado
- [ ] Puedo arrastrar tareas entre columnas
- [ ] Al mover una tarea a "Bloqueado", el sistema me pide justificación técnica (RF-07)
- [ ] Puedo filtrar tareas por proyecto

**Tareas Técnicas**:
9. **[TAREA 5.1]** Crear Tablero Kanban de Tareas
   - **Archivo**: `GestionObras.Web/Components/Pages/Kanban.razor`
   - **Diseño de columnas**:
     ```
     ┌─────────┬─────────┬─────────┬─────────┐
     │Pendiente│En Curso │Bloqueado│Finaliz. │
     ├─────────┼─────────┼─────────┼─────────┤
     │ [Tarea1]│ [Tarea4]│ [Tarea7]│ [Tarea9]│
     │ [Tarea2]│ [Tarea5]│         │         │
     │ [Tarea3]│ [Tarea6]│         │         │
     │         │         │         │         │
     │ [+ Nueva│         │         │         │
     │  Tarea] │         │         │         │
     └─────────┴─────────┴─────────┴─────────┘
     ```
   - **Tarjeta de Tarea**:
     - Título de la tarea
     - Descripción corta
     - Empleado asignado (avatar + nombre)
     - Fecha límite
     - Icono de prioridad
   - **Funcionalidad Drag & Drop**:
     - Usar librería JS o Blazor Component para arrastrar
     - Al soltar en nueva columna, actualizar estado en BD
   - **Modal de Bloqueo** (RF-07):
     ```
     ┌─────────────────────────────────────┐
     │  ⚠️ Justificar Bloqueo de Tarea     │
     ├─────────────────────────────────────┤
     │  Motivo:                            │
     │  ( ) Falta de material              │
     │  ( ) Error de ejecución             │
     │  ( ) Incidencia normativa           │
     │  ( ) Condiciones meteorológicas     │
     │                                     │
     │  Descripción detallada:             │
     │  [___________________________]      │
     │  [___________________________]      │
     │                                     │
     │  [Cancelar]  [Bloquear Tarea]       │
     └─────────────────────────────────────┘
     ```
   - **Filtros**:
     - Dropdown: "Todos los proyectos" / "Proyecto específico"
     - Búsqueda por nombre de tarea
   - **Estimación**: 8 horas
   - **Prioridad**: ALTA

---

### **HISTORIA DE USUARIO 6**: Como equipo de desarrollo, necesito documentar el trabajo realizado

**Criterios de Aceptación**:
- [ ] Existe un documento SPRINT_1.md con todos los objetivos alcanzados
- [ ] El README principal está actualizado con capturas de pantalla
- [ ] Se han documentado los problemas encontrados y sus soluciones

**Tareas Técnicas**:
10. **[TAREA 6.1]** Crear documentación del Sprint 1
   - **Secciones del documento**:
     1. Objetivos alcanzados ✅
     2. Arquitectura implementada (diagrama de capas)
     3. Capturas de pantalla:
        - Dashboard
        - Gestión de proyectos
        - Tablero Kanban
     4. Problemas técnicos encontrados:
        - Errores de migración
        - Configuración de Identity
        - Problemas de Drag & Drop
     5. Métricas del sprint:
        - Líneas de código añadidas
        - Tests escritos (si aplica)
        - Tiempo invertido por tarea
   - **Actualizar README.md**:
     - Añadir sección "Estado del Proyecto"
     - Incluir badges de build status
     - Actualizar roadmap
   - **Estimación**: 2 horas
   - **Prioridad**: MEDIA

---

## 📊 Estimación de Esfuerzo

| Tarea | Estimación | Prioridad | Dependencias |
|-------|------------|-----------|--------------|
| 1.1 - ApplicationDbContext | 4h | CRÍTICA | - |
| 1.2 - Connection String | 1h | CRÍTICA | 1.1 |
| 1.3 - Migraciones | 2h | CRÍTICA | 1.1, 1.2 |
| 2.1 - Identity + Roles | 3h | ALTA | 1.3 |
| 2.2 - Login/Registro | 5h | ALTA | 2.1 |
| 3.1 - Dashboard | 4h | ALTA | 2.2, 4.2 |
| 4.1 - CRUD Proyectos | 6h | CRÍTICA | 2.2, 4.2 |
| 4.2 - Repositorios | 4h | CRÍTICA | 1.3 |
| 5.1 - Kanban | 8h | ALTA | 4.2 |
| 6.1 - Documentación | 2h | MEDIA | Todas |
| **TOTAL** | **39 horas** | | |

**Distribución sugerida**: 
- Semana 1 (20h): Tareas 1.1 a 4.2 (Base de datos + Autenticación + Proyectos)
- Semana 2 (19h): Tareas 5.1 y 6.1 (Kanban + Documentación)

---

## 🎨 Diseño Visual de Referencia

### Paleta de Colores (Bootstrap + Personalizada)
```css
/* Tema Construction Management */
--primary: #FF6B35;      /* Naranja construcción */
--secondary: #004E89;    /* Azul corporativo */
--success: #2A9D8F;      /* Verde éxito */
--warning: #F4A261;      /* Amarillo alertas */
--danger: #E63946;       /* Rojo bloqueado */
--light: #F8F9FA;        /* Fondo claro */
--dark: #2B2D42;         /* Texto oscuro */
```

### Componentes UI a usar (Bootstrap 5)
- Cards (Dashboard, tarjetas de proyecto)
- Modals (formularios, confirmaciones)
- Tables (listado de proyectos)
- Badges (estados: Activo, Bloqueado, Finalizado)
- Progress Bars (ROI, avance de proyecto)
- Toast Notifications (alertas de éxito/error)

---

## ✅ Definition of Done (DoD)

Una tarea se considera **COMPLETADA** cuando:
1. ✅ El código está escrito y funciona correctamente
2. ✅ No hay errores de compilación ni warnings críticos
3. ✅ La funcionalidad cumple los criterios de aceptación
4. ✅ El código está comentado en secciones críticas
5. ✅ La interfaz es responsive (probado en móvil y desktop)
6. ✅ Los datos se persisten correctamente en la base de datos
7. ✅ Se ha probado manualmente con diferentes roles
8. ✅ Se ha actualizado la documentación técnica

---

## 🚧 Riesgos Identificados

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|--------------|---------|------------|
| Problemas con migraciones de EF Core | Media | Alto | Backup de BD antes de cada migración |
| Complejidad del Drag & Drop en Blazor | Alta | Medio | Usar librería probada (MudBlazor o Syncfusion) |
| Rendimiento del dashboard con muchos datos | Baja | Medio | Implementar paginación desde el inicio |
| Conflictos de configuración Identity | Media | Alto | Seguir documentación oficial de Microsoft |

---

## 📝 Notas Técnicas Importantes

### Orden de Desarrollo Recomendado:
```
1. Base de datos (sin esto, nada funciona)
   ↓
2. Autenticación (proteger las rutas)
   ↓
3. Repositorios (capa de acceso a datos)
   ↓
4. Páginas Blazor (interfaz de usuario)
   ↓
5. Testing y documentación
```

### Comandos Útiles:
```bash
# Crear migración
dotnet ef migrations add NombreMigracion --startup-project ../GestionObras.Web

# Aplicar migración
dotnet ef database update --startup-project ../GestionObras.Web

# Eliminar última migración (si hay error)
dotnet ef migrations remove --startup-project ../GestionObras.Web

# Ver estado de BD
dotnet ef migrations list --startup-project ../GestionObras.Web

# Compilar solución completa
dotnet build

# Ejecutar aplicación web
cd GestionObras.Web
dotnet run
```

---

## 🎯 Objetivo Final del Sprint 1

Al finalizar este sprint, deberías poder:

1. 🔐 **Hacer login** como Administrador
2. 📊 **Ver el dashboard** con tarjetas de resumen
3. 🏗️ **Crear un proyecto** con todos sus datos (ubicación, suelo, presupuesto)
4. 📋 **Ver el proyecto en una tabla** con su ROI calculado
5. 📌 **Crear tareas** en el tablero Kanban
6. 🎯 **Arrastrar tareas** entre las columnas Pendiente → En Curso → Finalizado
7. ⚠️ **Justificar bloqueos** cuando una tarea se mueve a "Bloqueado"
8. 📱 **Usar la aplicación** desde el móvil (diseño responsive)

---

## 🚀 Próximo Sprint (Adelanto)

**Sprint 2** se centrará en:
- Gestión completa de Empleados (con PRL y fichajes)
- Catálogo de Materiales con integración CTE
- Sistema de Facturas y proveedores
- Servicio de vigilancia del BOE (primer servicio de inteligencia normativa)

---

**¿Listo para empezar? Vamos con la primera tarea: Configurar ApplicationDbContext** 🔨
