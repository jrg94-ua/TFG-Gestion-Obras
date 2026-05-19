# Anexo: Flujos de Implementacion por Requisito Funcional

## 1. Objetivo del anexo

Este anexo sirve como apoyo tecnico para la defensa del TFG. Su finalidad es responder de forma directa a preguntas del tipo: "como se ha implementado este requisito" o "cual es el flujo tecnico de esta funcionalidad". Para cada requisito funcional implementado se recoge:

- el flujo principal de ejecucion,
- los archivos clave implicados,
- y un fragmento representativo de codigo real del repositorio.

## 2. Requisitos implementados

### RF-01. Gestion de proyectos

- Flujo: `Proyectos.razor -> ProyectosApiClient -> /api/proyectos -> GestionObrasDbContext -> SQL Server`
- Archivos clave:
  - `src/GestionObras.Web/Components/Pages/Proyectos.razor`
  - `src/GestionObras.Web/Services/ProyectosApiClient.cs`
  - `src/GestionObras.API/Program.cs`
- Explicacion: la interfaz delega la carga y el CRUD al cliente HTTP tipado, y la API resuelve el caso de uso antes de persistir.

```csharp
public async Task<OperacionResponse> GuardarProyectoAsync(GuardarProyectoRequest request, CancellationToken cancellationToken = default)
{
    using var response = await _httpClient.PostAsJsonAsync("/api/proyectos", request, cancellationToken);
    return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
           ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
}
```

### RF-02. Gestion visual de tareas en obra

- Flujo: `Kanban.razor -> KanbanService -> GestionObrasDbContext -> SQL Server`
- Archivos clave:
  - `src/GestionObras.Web/Components/Pages/Kanban.razor`
  - `src/GestionObras.Web/Services/KanbanService.cs`
- Explicacion: el componente Razor ya no persiste tareas directamente. La coordinacion de carga, guardado, bloqueo, dependencias y firmas se concentra en `KanbanService`.

```csharp
if (tareaEditando.Id > 0)
{
    var existente = await _db.Tareas
        .Include(t => t.UsuariosAsignados)
        .Include(t => t.Predecesoras)
        .FirstOrDefaultAsync(t => t.Id == tareaEditando.Id);
}
else
{
    tareaEditando.Predecesoras = dependencias;
    _db.Tareas.Add(tareaEditando);
}
```

### RF-03. Gestion de subtareas y jerarquia

- Flujo: `Tarea -> TareaPadre/SubTareas -> Kanban/MiTablero`
- Archivos clave:
  - `src/GestionObras.Core/Entities/Tarea.cs`
  - `src/GestionObras.Web/Components/Pages/MiTablero.razor`
- Explicacion: la jerarquia se modela en la propia entidad y luego se utiliza en las vistas para limitar operaciones y simplificar la experiencia del operario.

```csharp
public int? TareaPadreId { get; set; }
public Tarea? TareaPadre { get; set; }
public List<Tarea> SubTareas { get; set; } = new();
```

```csharp
if (tarea.RequiereFirmaConjunta && !tarea.TodosHanFirmado())
{
    throw new InvalidOperationException("La tarea requiere la firma de todos los usuarios asignados antes de finalizar.");
}
```

### RF-04. Control de dependencias entre tareas

- Flujo: `Kanban.razor -> KanbanService -> Predecesoras -> validacion antes del guardado y del cambio de estado`
- Archivos clave:
  - `src/GestionObras.Web/Services/KanbanService.cs`
  - `src/GestionObras.Core/Entities/Tarea.cs`
- Explicacion: las dependencias se resuelven desde el servicio de aplicacion y se asocian como predecesoras de la tarea editada.

```csharp
var dependencias = await _db.Tareas
    .Where(t => idsDependencias.Contains(t.Id))
    .ToListAsync();

existente.Predecesoras.Clear();
foreach (var dependencia in dependencias)
{
    existente.Predecesoras.Add(dependencia);
}
```

### RF-05. Gestion de bloqueos de produccion

- Flujo: `Kanban.razor -> KanbanService -> BloqueoTarea -> SQL Server`
- Archivos clave:
  - `src/GestionObras.Core/Entities/Tarea.cs`
  - `src/GestionObras.Web/Components/Pages/Kanban.razor`
- Explicacion: el bloqueo se modela como entidad propia asociada a la tarea, con tipo, justificacion y fechas.

```csharp
public class BloqueoTarea
{
    public int TareaId { get; set; }
    public TipoBloqueo Tipo { get; set; }
    public string JustificacionTecnica { get; set; } = string.Empty;
    public DateTime FechaBloqueo { get; set; }
}
```

### RF-06. Asignacion de personal a tareas

- Flujo: `Kanban.razor -> KanbanService -> UserManager/Users -> UsuariosAsignados/ResponsableFinal`
- Archivos clave:
  - `src/GestionObras.Web/Components/Pages/Kanban.razor`
  - `src/GestionObras.Core/Entities/Tarea.cs`
- Explicacion: la UI obliga a seleccionar un `ResponsableFinal` y permite asociar multiples usuarios como responsables de ejecucion.

```csharp
if (string.IsNullOrWhiteSpace(tareaEditando.ResponsableFinalId))
{
    await JS.InvokeVoidAsync("alert", "Selecciona un trabajador final para esta tarea.");
    return;
}

var responsables = responsablesSeleccionados.Any()
    ? await _db.Users
    .Where(u => responsablesSeleccionados.Contains(u.Id))
    .ToListAsync()
    : new List<UsuarioObra>();
tareaEditando.UsuariosAsignados = responsables;
```

### RF-07. Tablero personal del trabajador

- Flujo: `MiTablero.razor -> AdministracionApiClient -> /api/administracion/mi-tablero -> SQL Server`
- Archivos clave:
  - `src/GestionObras.Web/Components/Pages/MiTablero.razor`
  - `src/GestionObras.Web/Services/AdministracionApiClient.cs`
  - `src/GestionObras.API/Program.cs`
- Explicacion: el tablero personal se carga desde la API con un DTO ya filtrado por usuario y con operaciones concretas para cambiar estado, bloquear y finalizar.

```csharp
public async Task<MiTableroResponse> ObtenerMiTableroAsync(CancellationToken cancellationToken = default)
{
    return await _httpClient.GetFromJsonAsync<MiTableroResponse>("/api/administracion/mi-tablero", cancellationToken)
           ?? new MiTableroResponse();
}
```

### RF-08. Visualizacion temporal del proyecto

- Flujo: `GanttProyecto.razor -> ConsultasApiClient -> /api/consultas/proyectos/{id}/gantt`
- Archivos clave:
  - `src/GestionObras.Web/Components/Pages/GanttProyecto.razor`
  - `src/GestionObras.Web/Services/ConsultasApiClient.cs`
- Explicacion: la UI obtiene de la API el proyecto y sus tareas normalizadas para el diagrama, y a partir de ahi renderiza la linea temporal.

```csharp
public async Task<GanttProyectoResponse> ObtenerGanttProyectoAsync(int proyectoId, CancellationToken cancellationToken = default)
{
    return await _httpClient.GetFromJsonAsync<GanttProyectoResponse>($"/api/consultas/proyectos/{proyectoId}/gantt", cancellationToken)
           ?? new GanttProyectoResponse();
}
```

### RF-09. Gestion de materiales y proveedores

- Flujo: `Materiales.razor -> MaterialesApiClient -> /api/materiales/* -> SQL Server`
- Archivos clave:
  - `src/GestionObras.Web/Components/Pages/Materiales.razor`
  - `src/GestionObras.Web/Services/MaterialesApiClient.cs`
  - `src/GestionObras.API/Program.cs`
- Explicacion: el modulo obtiene catalogos y persiste cambios mediante llamadas HTTP tipadas; la API resuelve proveedor principal y relaciones.

```csharp
using var response = await _httpClient.PostAsJsonAsync("/api/materiales", request, cancellationToken);
return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
       ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
```

### RF-10. Solicitud y aprobacion de materiales

- Flujo: `SolicitarMateriales.razor -> MaterialesApiClient -> /api/materiales/solicitudes -> revision admin -> descuento de stock`
- Archivos clave:
  - `src/GestionObras.Web/Components/Pages/JefeObra/SolicitarMateriales.razor`
  - `src/GestionObras.Web/Components/Pages/Admin/GestionSolicitudesMateriales.razor`
- Explicacion: primero se crea la solicitud desde jefatura de obra y despues el administrador la aprueba o rechaza; en la aprobacion se actualiza el stock.

```csharp
using var response = await _httpClient.PostAsJsonAsync("/api/materiales/solicitudes", request, cancellationToken);
return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
       ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
```

```csharp
var material = await db.Materiales.FindAsync(solicitud.MaterialId);
if (material != null)
{
    material.StockDisponible -= (int)solicitud.CantidadSolicitada;
}
```

### RF-11. Gestion de presupuestos

- Flujo: `Presupuestos.razor -> PresupuestoService -> DbContext.Presupuestos`
- Archivos clave:
  - `src/GestionObras.Web/Components/Pages/Presupuestos.razor`
  - `src/GestionObras.Web/Services/PresupuestoService.cs`
- Explicacion: la pagina se apoya en un servicio de aplicacion que encapsula la lectura y escritura de presupuestos.

```csharp
public async Task GuardarAsync(Presupuesto presupuesto)
{
    if (presupuesto.Id == 0)
        _db.Presupuestos.Add(presupuesto);
    else
        _db.Presupuestos.Update(presupuesto);

    await _db.SaveChangesAsync();
}
```

### RF-12. Gestion de facturas y gastos

- Flujo: `Facturas.razor -> FacturaService -> recalculo economico -> DbContext.Facturas`
- Archivos clave:
  - `src/GestionObras.Web/Components/Pages/Facturas.razor`
  - `src/GestionObras.Web/Services/FacturaService.cs`
- Explicacion: la logica economica no se deja en la UI, sino que se centraliza en `FacturaService`.

```csharp
private static void RecalcularImportes(Factura factura)
{
    var descuento = factura.BaseImponible * (factura.DescuentoPorcentaje / 100);
    var baseNeta = factura.BaseImponible - descuento;

    factura.IVA = baseNeta * (factura.PorcentajeIVA / 100);
    factura.ImporteTotal = baseNeta + factura.IVA;
}
```

### RF-13. Exportacion documental

- Flujo: `pagina Razor -> ExportPdfService / ExportExcelService -> descarga en navegador`
- Archivos clave:
  - `src/GestionObras.Web/Services/ExportPdfService.cs`
  - `src/GestionObras.Web/Services/ExportExcelService.cs`
  - `src/GestionObras.Web/Components/Pages/Facturas.razor`
- Explicacion: cada modulo construye un conjunto de datos y delega la generacion documental a servicios transversales.

```csharp
var bytes = ExportPdf.GenerarInformeFacturas(datos);
await JS.InvokeVoidAsync("descargarArchivo", $"Facturas_{DateTime.Now:yyyyMMdd}.pdf", "application/pdf", base64);
```

```csharp
return Document.Create(container =>
{
    container.Page(page =>
    {
        page.Header().Element(c => ComponerCabecera(c, titulo));
        page.Content().Element(c => ComponerTablaFacturas(c, facturas));
    });
}).GeneratePdf();
```

### RF-14. Gestion de contratos

- Flujo: `RRHHContratos.razor -> RRHHApiClient -> /api/rrhh/contratos -> SQL Server`
- Archivos clave:
  - `src/GestionObras.Web/Components/Pages/RRHH/RRHHContratos.razor`
  - `src/GestionObras.Web/Services/RRHHApiClient.cs`
  - `src/GestionObras.API/Program.cs`
- Explicacion: la pantalla gestiona contratos a traves de la API, que encapsula alta, edicion y finalizacion.

```csharp
using var response = await _httpClient.PostAsJsonAsync("/api/rrhh/contratos", request, cancellationToken);
return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
       ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
```

```csharp
app.MapPost("/api/rrhh/contratos/{id:int}/finalizar", async (int id, GestionObrasDbContext db) =>
{
    var contrato = await db.Contratos.FindAsync(id);
    contrato!.Activo = false;
    await db.SaveChangesAsync();
});
```

### RF-15. Gestion de horarios

- Flujo: `RRHHHorarios / FichajeRepository -> HorariosAsignados -> vista semanal`
- Archivos clave:
  - `src/GestionObras.Infrastructure/Repositories/FichajeRepository.cs`
  - `src/GestionObras.Web/Components/Pages/RRHH/RRHHHorarios.razor`
- Explicacion: los horarios se consultan y agrupan por usuario, proyecto, dia y vigencia.

```csharp
public async Task<List<HorarioAsignado>> GetHorariosByUsuarioAsync(string usuarioId)
{
    return await _context.HorariosAsignados
        .Include(h => h.Proyecto)
        .Where(h => h.UsuarioId == usuarioId && h.Activo)
        .OrderBy(h => h.DiaSemana)
        .ThenBy(h => h.HoraEntrada)
        .ToListAsync();
}
```

### RF-16. Generacion automatica de horarios

- Flujo: `RRHHHorarios.razor -> PlanificacionHorarioService -> HorariosAsignados`
- Archivos clave:
  - `src/GestionObras.Web/Components/Pages/RRHH/RRHHHorarios.razor`
  - `src/GestionObras.Web/Services/PlanificacionHorarioService.cs`
- Explicacion: la generacion se basa en horas estimadas de tareas, contrato, limite diario y perfil operativo del responsable final.

```csharp
var resultado = await PlanificacionHorarioService.GenerarHorariosAutomaticosAsync(ProyectoSeleccionado.Id);
ultimoResultadoGeneracion = resultado;
await CargarHorarios();
```

```csharp
var tareasNoOperativas = tareasActivas
    .Where(t => !t.ResponsableFinal.EsPerfilOperativo())
    .ToList();
```

### RF-17. Registro de fichajes

- Flujo: `Operario/Fichaje.razor -> OperarioApiClient -> /api/operario/fichaje/* -> SQL Server`
- Archivos clave:
  - `src/GestionObras.Web/Components/Pages/Operario/Fichaje.razor`
  - `src/GestionObras.Web/Services/OperarioApiClient.cs`
  - `src/GestionObras.API/Program.cs`
- Explicacion: el operario registra entrada y salida mediante API; RRHH valida o rechaza posteriormente el fichaje desde su propio modulo.

```csharp
using var response = await _httpClient.PostAsJsonAsync(
    "/api/operario/fichaje/entrada",
    new CrearFichajeRequest { ProyectoId = proyectoId },
    cancellationToken);
```

```csharp
var fichaje = new RegistroFichaje
{
    UsuarioId = usuario.Id,
    ProyectoId = request.ProyectoId,
    Fecha = DateOnly.FromDateTime(ahora),
    HoraEntrada = ahora,
    Estado = EstadoFichaje.Pendiente
};
db.RegistrosFichajes.Add(fichaje);
```

```csharp
fichaje.HoraSalida = ahora;
await db.SaveChangesAsync();
```

### RF-18. Autenticacion y autorizacion por roles

- Flujo: `Login.razor -> SignInManager -> Program.cs -> Identity roles -> navegacion filtrada`
- Archivos clave:
  - `src/GestionObras.Web/Components/Pages/Login.razor`
  - `src/GestionObras.Web/Program.cs`
  - `src/GestionObras.Web/Components/Layout/NavMenu.razor`
- Explicacion: el login autentica al usuario y la aplicacion inicializa roles del sistema desde el arranque.

```csharp
var result = await SignInManager.PasswordSignInAsync(
    loginModel.Email,
    loginModel.Password,
    loginModel.RememberMe,
    lockoutOnFailure: false
);
```

```csharp
foreach (var rol in roles)
{
    if (!await roleManager.RoleExistsAsync(rol))
        await roleManager.CreateAsync(new IdentityRole(rol));
}
```

### RF-19. Administracion de usuarios y perfiles

- Flujo: `Admin/Usuarios.razor -> AdministracionApiClient -> /api/administracion/usuarios -> UserManager/RoleManager`
- Archivos clave:
  - `src/GestionObras.Web/Components/Pages/Admin/Usuarios.razor`
  - `src/GestionObras.Web/Services/AdministracionApiClient.cs`
  - `src/GestionObras.API/Program.cs`
- Explicacion: la gestion administrativa se invoca desde la UI mediante API; la creacion y reasignacion de roles se ejecuta en backend con `UserManager` y `RoleManager`.

```csharp
using var response = await _httpClient.PostAsJsonAsync("/api/administracion/usuarios", request, cancellationToken);
return await response.Content.ReadFromJsonAsync<OperacionResponse>(cancellationToken: cancellationToken)
       ?? new OperacionResponse { Correcto = response.IsSuccessStatusCode, Mensaje = "Respuesta vacia" };
```

```csharp
var result = await userManager.CreateAsync(usuario, request.Password);
if (result.Succeeded && !string.IsNullOrWhiteSpace(request.Rol))
{
    await userManager.AddToRoleAsync(usuario, request.Rol);
}
```

## 3. Requisitos parciales

### RF-20. Control de stock ligado a operativa de obra

- Estado: parcial.
- Explicacion: existe control de stock y descuento por solicitudes aprobadas, pero no consumo automatico por ejecucion de tarea.

### RF-21. Registro de jornada con geolocalizacion

- Estado: parcial.
- Explicacion: la entidad `RegistroFichaje` soporta latitud y longitud, pero la interfaz actual no captura esas coordenadas.

### PRL. Validacion de formacion preventiva

- Estado: parcial.
- Explicacion: el dominio `Empleado` modela cursos PRL y su vigencia, pero la validacion no esta integrada de forma uniforme en todos los flujos operativos.

### Normativa. Inteligencia documental y legal

- Estado: parcial.
- Explicacion: existen contratos de servicio y documentacion conceptual, pero no una integracion funcional cerrada con BOE, CTE o PGOU en la aplicacion ejecutable.

### RF-22. Carpeta documental del proyecto

- Estado: parcial.
- Explicacion: existe la entidad `CarpetaLegal`, pero no un modulo completo de explotacion funcional.

## 4. Uso recomendado en la defensa

La forma mas util de emplear este anexo en una exposicion oral es:

1. identificar el RF preguntado,
2. explicar el flujo `pantalla -> servicio/repositorio -> entidad -> base de datos`,
3. citar uno o dos archivos clave,
4. y apoyar la respuesta con el fragmento de codigo correspondiente.
