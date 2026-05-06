# Tests

Esta carpeta contiene la suite formal de pruebas automatizadas del proyecto.

## Estructura real

```text
tests/
|-- GestionObras.UnitTests/
|   |-- Domain/
|   `-- Services/
`-- GestionObras.IntegrationTests/
    `-- Repositories/
```

## Cobertura inicial

La suite creada cubre actualmente:

- reglas de dominio de `Proyecto` y `Tarea`,
- calculo y persistencia de `FacturaService`,
- generacion automatica de horarios en `PlanificacionHorarioService`,
- y consultas/borrado del `FichajeRepository`.

## Tecnologias de testing

- `xUnit` como framework principal.
- `FluentAssertions` para aserciones legibles.
- `EF Core InMemory` para pruebas unitarias de servicios.
- `SQLite in-memory` para pruebas de integracion de persistencia.

## Ejecucion

```powershell
dotnet test TFG-JORGE.sln
dotnet test src/GestionObras.slnx
```

Requisito: disponer del SDK de `.NET 10.0`, que es el `TargetFramework` del proyecto principal y de la suite de tests.

Si el equipo local no tiene ese SDK, puede ejecutarse desde Docker:

```powershell
docker run --rm -v "${PWD}:/workspace" -w /workspace mcr.microsoft.com/dotnet/sdk:10.0 dotnet test TFG-JORGE.sln
```

Resultado validado en contenedor:

- `GestionObras.UnitTests`: `12/12` pruebas superadas.
- `GestionObras.IntegrationTests`: `3/3` pruebas superadas.
- Total actual: `15` pruebas superadas, `0` fallidas.

## Limites actuales

La suite todavia no cubre:

- pruebas end-to-end de interfaz,
- reglas avanzadas del kanban con drag and drop,
- exportaciones PDF/Excel,
- flujos completos de autenticacion por UI,
- ni pruebas de carga o rendimiento.

Estas areas deben figurar como trabajo futuro, no como cobertura ya alcanzada.
