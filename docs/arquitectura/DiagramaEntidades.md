# Diagrama de Entidades del Dominio

```mermaid
classDiagram
    class Proyecto {
        +int Id
        +string Nombre
        +Ubicacion Ubicacion
        +TipoSuelo TipoSuelo
        +DateTime FechaInicio
        +DateTime? FechaFin
        +EstadoProyecto Estado
        +Presupuesto Presupuesto
        +CarpetaLegal CarpetaLegal
        +List~Tarea~ Tareas
        +List~Empleado~ EmpleadosAsignados
        +CalcularROIActual() decimal
        +PuedeIniciarTarea(Tarea) bool
    }
    
    class Ubicacion {
        <<ValueObject>>
        +string Provincia
        +string Municipio
        +CoordenadaGPS Coordenadas
        +ZonaClimatica ZonaClimatica
    }
    
    class Tarea {
        +int Id
        +string Nombre
        +string Descripcion
        +EstadoTarea Estado
        +DateTime FechaInicio
        +DateTime? FechaFin
        +decimal PresupuestoEstimado
        +decimal CostesReales
        +List~Material~ MaterialesNecesarios
        +List~Empleado~ Responsables
        +BloqueoTarea? Bloqueo
    }
    
    class Material {
        +int Id
        +string Codigo
        +string Nombre
        +decimal TransmitanciaTermica
        +string ClasificacionFuego
        +decimal PrecioUnitario
        +int StockDisponible
        +ValidarCumplimientoCTE() bool
    }
    
    class Empleado {
        +int Id
        +string Nombre
        +string DNI
        +CategoriaLaboral Categoria
        +List~CursoPRL~ CursosPRL
        +List~Fichaje~ Fichajes
        +TienePRLVigente() bool
    }
    
    class CursoPRL {
        +int Id
        +string NombreCurso
        +DateTime FechaRealizacion
        +DateTime FechaExpiracion
        +bool EstaVigente() bool
    }
    
    class Factura {
        +int Id
        +string NumeroFactura
        +decimal Importe
        +DateTime FechaEmision
        +DateTime? FechaPago
        +EstadoFactura Estado
        +Proveedor Proveedor
        +Tarea TareaRelacionada
    }
    
    class CarpetaLegal {
        +int Id
        +List~DocumentoNormativo~ Documentos
        +DateTime FechaCreacion
        +DateTime? UltimaActualizacion
        +AgregarDocumento(DocumentoNormativo)
    }
    
    Proyecto "1" --> "1" Ubicacion
    Proyecto "1" --> "*" Tarea
    Proyecto "1" --> "1" CarpetaLegal
    Proyecto "1" --> "*" Empleado
    Tarea "*" --> "*" Material
    Tarea "*" --> "*" Empleado
    Tarea "1" --> "*" Factura
    Empleado "1" --> "*" CursoPRL
```

## Enumeraciones Principales

### EstadoProyecto
- Planificacion
- EnCurso
- Bloqueado
- Finalizado
- Cancelado

### EstadoTarea
- Pendiente
- EnCurso
- Bloqueado
- Finalizado

### TipoSuelo
- Urbano
- Rustico

### CategoriaLaboral
- Peon
- OficialSegunda
- OficialPrimera
- Encargado
- JefeObra

---

**Versión**: 1.0  
**Autor**: Jorge Ros Gómez
