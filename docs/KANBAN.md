# Tablero Kanban Jerárquico

## Descripción

El sistema de gestión de obras incluye un **tablero Kanban jerárquico** para cada proyecto, permitiendo una visualización intuitiva del estado de las tareas y su organización en múltiples niveles.

## Características Principales

### 🔹 Jerarquía de Tareas

- **Tareas Raíz (Nivel 0)**: Tareas principales del proyecto
- **Subtareas (Nivel 1+)**: Tareas dependientes que se anidan bajo las tareas padre
- **Visualización jerárquica**: Las subtareas se indican visualmente con indentación y marcadores
- **Expansión/Colapso**: Control para mostrar u ocultar subtareas

### 🔹 Columnas del Tablero

El tablero se organiza en 4 columnas según el estado:

1. **📝 Pendiente**: Tareas por iniciar
2. **⚙️ En Curso**: Tareas en ejecución
3. **🚫 Bloqueado**: Tareas con impedimentos (requiere justificación)
4. **✅ Finalizado**: Tareas completadas

### 🔹 Funcionalidad Drag & Drop

- Arrastre de tarjetas entre columnas para cambiar el estado
- Actualización automática en base de datos
- Validación de bloqueos (RF-07)

### 🔹 Gestión de Bloqueos (RF-07)

Cuando una tarea se bloquea, es **obligatorio** proporcionar:

- **Tipo de bloqueo**:
  - Falta de Material
  - Error de Ejecución
  - Incidencia Normativa
  - Climatología Adversa
  - Otro
  
- **Justificación técnica**: Descripción detallada del motivo
- **Fecha de bloqueo**: Registro automático
- **Fecha de resolución**: Se registra al desbloquear

### 🔹 Prioridades de Tareas

Las tareas se pueden clasificar por prioridad:

- **Baja** (badge gris)
- **Media** (badge azul) - Por defecto
- **Alta** (badge amarillo)
- **Crítica** (badge rojo)

Las tareas se ordenan automáticamente por prioridad dentro de cada columna.

## Acceso al Tablero

### Desde la Lista de Proyectos

En la página `/proyectos`, cada proyecto tiene un botón con icono de Kanban:

```
[🔲] [👁️] [✏️] [🗑️]
```

### Desde el Dashboard del Jefe de Obra

- Botón "Tablero Kanban" en acciones rápidas
- Si hay un solo proyecto, navega directo al tablero
- Si hay múltiples proyectos, redirige a la lista para seleccionar

### URL Directa

```
/proyectos/{ProyectoId}/kanban
```

Ejemplo: `/proyectos/1/kanban`

## Estructura de Datos

### Entidad Tarea (Actualizada)

```csharp
public class Tarea
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public EstadoTarea Estado { get; set; }
    public PrioridadTarea Prioridad { get; set; }
    
    // Fechas
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    
    // Presupuesto
    public decimal PresupuestoEstimado { get; set; }
    public decimal CostesReales { get; set; }
    
    // Jerarquía - NUEVO
    public int? TareaPadreId { get; set; }
    public Tarea? TareaPadre { get; set; }
    public List<Tarea> SubTareas { get; set; }
    public int Nivel { get; set; } // 0 = raíz, 1+ = subtarea
    
    // Relaciones
    public int ProyectoId { get; set; }
    public Proyecto Proyecto { get; set; }
    public BloqueoTarea? Bloqueo { get; set; }
    public List<Empleado> Responsables { get; set; }
}
```

### Entidad BloqueoTarea

```csharp
public class BloqueoTarea
{
    public int Id { get; set; }
    public int TareaId { get; set; }
    public TipoBloqueo Tipo { get; set; }
    public string JustificacionTecnica { get; set; }
    public DateTime FechaBloqueo { get; set; }
    public DateTime? FechaResolucion { get; set; }
}
```

## Uso del Tablero

### 1. Crear Nueva Tarea

- Click en "Nueva Tarea"
- Completar formulario:
  - Nombre (obligatorio)
  - Descripción
  - Estado
  - Prioridad
  - Fechas inicio/fin
  - Presupuesto

### 2. Crear Subtarea

- En cualquier tarjeta, click en botón ➕ (Agregar subtarea)
- La subtarea heredará el proyecto y se vinculará a la tarea padre
- El nivel se incrementa automáticamente

### 3. Mover Tareas (Drag & Drop)

- Arrastrar tarjeta a otra columna
- Si se arrastra a "Bloqueado", aparece modal de justificación
- El estado se actualiza automáticamente

### 4. Bloquear Tarea

Al mover una tarea a "Bloqueado":

1. Aparece modal de justificación
2. Seleccionar tipo de bloqueo
3. Escribir justificación técnica
4. Click en "Bloquear Tarea"

El bloqueo queda registrado con fecha y tipo.

### 5. Desbloquear Tarea

- En tarjetas bloqueadas, aparece botón 🔓
- Click para ver detalles del bloqueo
- Confirmar desbloqueo
- Se registra fecha de resolución

### 6. Filtros y Búsqueda

- **Buscar**: Campo de texto para filtrar por nombre o descripción
- **Mostrar subtareas**: Switch para expandir/colapsar subtareas

## Migración de Base de Datos

Para aplicar los cambios de jerarquía, ejecutar:

```sql
-- Agregar columnas para jerarquía
ALTER TABLE Tareas ADD TareaPadreId INT NULL;
ALTER TABLE Tareas ADD Nivel INT NOT NULL DEFAULT 0;
ALTER TABLE Tareas ADD Prioridad INT NOT NULL DEFAULT 1;

-- Crear relación auto-referencial
ALTER TABLE Tareas 
ADD CONSTRAINT FK_Tareas_Tareas_TareaPadreId 
FOREIGN KEY (TareaPadreId) REFERENCES Tareas(Id);

-- Crear tabla de bloqueos
CREATE TABLE BloqueosTareas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TareaId INT NOT NULL,
    Tipo INT NOT NULL,
    JustificacionTecnica NVARCHAR(MAX) NOT NULL,
    FechaBloqueo DATETIME2 NOT NULL,
    FechaResolucion DATETIME2 NULL,
    CONSTRAINT FK_BloqueosTareas_Tareas_TareaId 
    FOREIGN KEY (TareaId) REFERENCES Tareas(Id) 
    ON DELETE CASCADE
);
```

## Componentes

### Kanban.razor
- Página principal del tablero
- Gestiona estado y lógica de negocio
- Implementa drag & drop
- Modales de creación/edición/bloqueo

### TarjetaTarea.razor
- Componente reutilizable para visualizar tareas
- Renderizado recursivo para subtareas
- Acciones: editar, eliminar, agregar subtarea, desbloquear

### kanban.css
- Estilos del tablero
- Diseño de 4 columnas
- Animaciones y efectos hover
- Responsive design

## Requisitos Funcionales Implementados

- ✅ **RF-05**: Tablero Kanban por proyecto
- ✅ **RF-07**: Justificación obligatoria de bloqueos
- ✅ **RF-08**: Jerarquía de tareas (tareas y subtareas)
- ✅ Drag & Drop entre estados
- ✅ Filtrado y búsqueda
- ✅ Gestión de prioridades

## Próximas Mejoras

- [ ] Asignación de responsables desde el tablero
- [ ] Filtro por responsable
- [ ] Filtro por prioridad
- [ ] Estadísticas del tablero (% completado, tiempo promedio)
- [ ] Límites WIP (Work In Progress) por columna
- [ ] Notificaciones de bloqueos
- [ ] Historial de cambios de estado
- [ ] Exportación del tablero a PDF/Excel
