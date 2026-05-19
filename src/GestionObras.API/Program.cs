using GestionObras.Core.Contracts.Administracion;
using GestionObras.Core.Contracts.Consultas;
using GestionObras.Core.Contracts.JefeObra;
using GestionObras.Core.Contracts.Materiales;
using GestionObras.Core.Contracts.Operario;
using GestionObras.Core.Contracts.Proyectos;
using GestionObras.Core.Contracts.RRHH;
using GestionObras.Core.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using GestionObras.Infrastructure.Data;
using GestionObras.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
var dataProtectionPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "shared-keys"));
Directory.CreateDirectory(dataProtectionPath);
var defaultConnectionString =
    builder.Configuration.GetConnectionString("DefaultConnection") ??
    builder.Configuration["ConnectionStrings:DefaultConnection"] ??
    throw new InvalidOperationException("No se ha configurado la cadena de conexion 'DefaultConnection'.");

builder.Services.AddOpenApi();
builder.Services.AddDbContext<GestionObrasDbContext>(options =>
    options.UseSqlServer(
        defaultConnectionString,
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(30),
        errorNumbersToAdd: null)));
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
    .SetApplicationName("GestionObras.Auth");
builder.Services.AddIdentityCore<UsuarioObra>()
    .AddRoles<IdentityRole>()
    .AddSignInManager()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<GestionObrasDbContext>();
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.Cookie.Name = "GestionObras.Auth";
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("JefeObraPolicy", policy => policy.RequireRole("JefeObra", "Administrador"));
    options.AddPolicy("OficinaTecnicaPolicy", policy => policy.RequireRole("OficinaTecnica", "Administrador"));
    options.AddPolicy("RecursosHumanosPolicy", policy => policy.RequireRole("RecursosHumanos", "Administrador"));
    options.AddPolicy("OperarioPolicy", policy => policy.RequireRole("Operario", "OperarioObra", "OperarioOficinaT", "JefeObra", "OficinaTecnica", "RecursosHumanos", "Administrador"));
});
builder.Services.AddScoped<TareaWorkflowService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GestionObrasDbContext>();
    dbContext.Database.SetConnectionString(defaultConnectionString);
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/api"))
    {
        await next();
        return;
    }

    if (!(context.User.Identity?.IsAuthenticated ?? false))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { correcto = false, mensaje = "Acceso no autenticado." });
        return;
    }

    if (!UsuarioPuedeAccederRuta(context.User, context.Request.Path))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new { correcto = false, mensaje = "No tienes permisos para acceder a este recurso." });
        return;
    }

    await next();
});

app.MapGet("/api/jefe-obra/horarios", async (
    HttpContext httpContext,
    int? proyectoId,
    string? usuarioId,
    GestionObrasDbContext db) =>
{
    var responsableId = ObtenerUsuarioAutenticadoId(httpContext.User);
    if (string.IsNullOrWhiteSpace(responsableId))
    {
        return Results.Unauthorized();
    }

    var misProyectos = await db.Proyectos
        .Where(p => p.ResponsableId == responsableId)
        .OrderBy(p => p.Nombre)
        .Select(p => new ProyectoResumenDto(p.Id, p.Nombre))
        .ToListAsync();

    var proyectoIds = misProyectos.Select(p => p.Id).ToList();
    if (!proyectoIds.Any())
    {
        return Results.Ok(new JefeObraHorariosResponse());
    }

    var operarios = await db.Users
        .Where(u => u.Activo && UsuarioPerfilRules.TiposOperativos.Contains(u.TipoUsuario))
        .OrderBy(u => u.NombreCompleto)
        .Select(u => new UsuarioResumenDto(u.Id, u.NombreCompleto, u.Cargo))
        .ToListAsync();

    var query = db.HorariosAsignados
        .Include(h => h.Usuario)
        .Include(h => h.Proyecto)
        .Where(h => h.Activo &&
                    h.ProyectoId.HasValue &&
                    proyectoIds.Contains(h.ProyectoId.Value));

    if (proyectoId.HasValue)
    {
        query = query.Where(h => h.ProyectoId == proyectoId.Value);
    }

    if (!string.IsNullOrWhiteSpace(usuarioId))
    {
        query = query.Where(h => h.UsuarioId == usuarioId);
    }

    var horarios = await query
        .OrderBy(h => h.Usuario!.NombreCompleto)
        .ThenBy(h => h.DiaSemana)
        .ThenBy(h => h.HoraEntrada)
        .Select(h => new HorarioAsignadoDto(
            h.Id,
            h.UsuarioId,
            h.Usuario!.NombreCompleto,
            h.Usuario.Cargo,
            h.ProyectoId,
            h.Proyecto != null ? h.Proyecto.Nombre : null,
            h.DiaSemana,
            h.HoraEntrada,
            h.HoraSalida,
            h.TipoTurno,
            h.HorasPrevistas))
        .ToListAsync();

    return Results.Ok(new JefeObraHorariosResponse
    {
        Proyectos = misProyectos,
        Operarios = operarios,
        Horarios = horarios
    });
})
.WithName("GetJefeObraHorarios");

app.MapGet("/api/jefe-obra/fichajes", async (
    HttpContext httpContext,
    DateOnly desde,
    DateOnly hasta,
    int? proyectoId,
    GestionObrasDbContext db) =>
{
    var responsableId = ObtenerUsuarioAutenticadoId(httpContext.User);
    if (string.IsNullOrWhiteSpace(responsableId))
    {
        return Results.Unauthorized();
    }

    var misProyectos = await db.Proyectos
        .Where(p => p.ResponsableId == responsableId)
        .OrderBy(p => p.Nombre)
        .Select(p => new ProyectoResumenDto(p.Id, p.Nombre))
        .ToListAsync();

    var proyectoIds = misProyectos.Select(p => p.Id).ToList();
    if (!proyectoIds.Any())
    {
        return Results.Ok(new JefeObraFichajesResponse());
    }

    var query = db.RegistrosFichaje
        .Include(f => f.Usuario)
        .Include(f => f.Proyecto)
        .Where(f => f.Fecha >= desde && f.Fecha <= hasta)
        .Where(f => f.ProyectoId.HasValue && proyectoIds.Contains(f.ProyectoId.Value));

    if (proyectoId.HasValue)
    {
        query = query.Where(f => f.ProyectoId == proyectoId.Value);
    }

    var fichajes = await query
        .OrderByDescending(f => f.Fecha)
        .ThenByDescending(f => f.HoraEntrada)
        .Select(f => new RegistroFichajeDto(
            f.Id,
            f.UsuarioId,
            f.Usuario!.NombreCompleto,
            f.Fecha,
            f.HoraEntrada,
            f.HoraSalida,
            f.ProyectoId,
            f.Proyecto != null ? f.Proyecto.Nombre : null,
            f.Estado,
            f.HoraSalida.HasValue ? (f.HoraSalida.Value - f.HoraEntrada).TotalHours : null))
        .ToListAsync();

    return Results.Ok(new JefeObraFichajesResponse
    {
        Proyectos = misProyectos,
        Fichajes = fichajes
    });
})
.WithName("GetJefeObraFichajes");

app.MapGet("/api/operario/dashboard", async (
    HttpContext httpContext,
    GestionObrasDbContext db) =>
{
    var usuarioId = ObtenerUsuarioAutenticadoId(httpContext.User);
    if (string.IsNullOrWhiteSpace(usuarioId))
    {
        return Results.Unauthorized();
    }

    var usuario = await db.Users.FirstOrDefaultAsync(u => u.Id == usuarioId && u.Activo);
    if (usuario == null)
    {
        return Results.NotFound();
    }

    var tareasPendientes = await db.Tareas.CountAsync(t => t.UsuariosAsignados.Any(u => u.Id == usuarioId) && t.Estado == EstadoTarea.Pendiente);
    var tareasEnCurso = await db.Tareas.CountAsync(t => t.UsuariosAsignados.Any(u => u.Id == usuarioId) && t.Estado == EstadoTarea.EnCurso);
    var tareasCompletadas = await db.Tareas.CountAsync(t => t.UsuariosAsignados.Any(u => u.Id == usuarioId) && t.Estado == EstadoTarea.Finalizado);

    var tareasActivas = await db.Tareas
        .Include(t => t.Proyecto)
        .Where(t => t.UsuariosAsignados.Any(u => u.Id == usuarioId) &&
                    (t.Estado == EstadoTarea.Pendiente || t.Estado == EstadoTarea.EnCurso))
        .OrderBy(t => t.FechaInicio)
        .Take(10)
        .Select(t => new TareaResumenDto(
            t.Id,
            t.Nombre,
            t.Proyecto != null ? t.Proyecto.Nombre : null,
            t.Estado,
            t.FechaInicio))
        .ToListAsync();

    var horarios = await db.HorariosAsignados
        .Include(h => h.Proyecto)
        .Where(h => h.UsuarioId == usuarioId && h.Activo)
        .OrderBy(h => h.DiaSemana)
        .ThenBy(h => h.HoraEntrada)
        .Select(h => new HorarioResumenDto(
            h.Id,
            h.DiaSemana,
            h.HoraEntrada,
            h.HoraSalida,
            h.TipoTurno,
            h.HorasPrevistas,
            h.Proyecto != null ? h.Proyecto.Nombre : null))
        .ToListAsync();

    var ultimoFichaje = await db.RegistrosFichaje
        .Where(f => f.UsuarioId == usuarioId)
        .OrderByDescending(f => f.HoraEntrada)
        .Select(f => new RegistroFichajeResumenDto(
            f.Id,
            f.Fecha,
            f.HoraEntrada,
            f.HoraSalida,
            f.Proyecto != null ? f.Proyecto.Nombre : null,
            f.Estado,
            f.HoraSalida.HasValue ? (f.HoraSalida.Value - f.HoraEntrada).TotalHours : null))
        .FirstOrDefaultAsync();

    return Results.Ok(new OperarioDashboardResponse
    {
        NombreUsuario = usuario.NombreCompleto?.Split(' ').FirstOrDefault() ?? "Operario",
        TareasPendientes = tareasPendientes,
        TareasEnCurso = tareasEnCurso,
        TareasCompletadas = tareasCompletadas,
        HorasSemanales = horarios.Sum(h => h.HorasPrevistas),
        TareasActivas = tareasActivas,
        HorariosSemanales = horarios,
        UltimoFichaje = ultimoFichaje
    });
})
.WithName("GetOperarioDashboard");

app.MapGet("/api/operario/fichaje", async (
    HttpContext httpContext,
    GestionObrasDbContext db) =>
{
    var usuarioId = ObtenerUsuarioAutenticadoId(httpContext.User);
    if (string.IsNullOrWhiteSpace(usuarioId))
    {
        return Results.Unauthorized();
    }

    var usuario = await db.Users.FirstOrDefaultAsync(u => u.Id == usuarioId && u.Activo);
    if (usuario == null)
    {
        return Results.NotFound();
    }

    var hoy = DateOnly.FromDateTime(DateTime.Today);
    var fichajeAbierto = await db.RegistrosFichaje
        .Include(f => f.Proyecto)
        .Where(f => f.UsuarioId == usuarioId && f.Fecha == hoy && f.HoraSalida == null)
        .Select(f => new RegistroFichajeResumenDto(
            f.Id,
            f.Fecha,
            f.HoraEntrada,
            f.HoraSalida,
            f.Proyecto != null ? f.Proyecto.Nombre : null,
            f.Estado,
            null))
        .FirstOrDefaultAsync();

    var historial = await db.RegistrosFichaje
        .Include(f => f.Proyecto)
        .Where(f => f.UsuarioId == usuarioId && f.Fecha >= hoy.AddDays(-7) && f.Fecha <= hoy)
        .OrderByDescending(f => f.Fecha)
        .ThenByDescending(f => f.HoraEntrada)
        .Select(f => new RegistroFichajeResumenDto(
            f.Id,
            f.Fecha,
            f.HoraEntrada,
            f.HoraSalida,
            f.Proyecto != null ? f.Proyecto.Nombre : null,
            f.Estado,
            f.HoraSalida.HasValue ? (f.HoraSalida.Value - f.HoraEntrada).TotalHours : null))
        .ToListAsync();

    var diaSemana = DateTime.Now.DayOfWeek switch
    {
        DayOfWeek.Monday => DiaSemana.Lunes,
        DayOfWeek.Tuesday => DiaSemana.Martes,
        DayOfWeek.Wednesday => DiaSemana.Miercoles,
        DayOfWeek.Thursday => DiaSemana.Jueves,
        DayOfWeek.Friday => DiaSemana.Viernes,
        DayOfWeek.Saturday => DiaSemana.Sabado,
        DayOfWeek.Sunday => DiaSemana.Domingo,
        _ => DiaSemana.Lunes
    };

    var horarioHoy = await db.HorariosAsignados
        .Include(h => h.Proyecto)
        .Where(h => h.UsuarioId == usuarioId && h.Activo && h.DiaSemana == diaSemana)
        .OrderBy(h => h.HoraEntrada)
        .Select(h => new HorarioResumenDto(
            h.Id,
            h.DiaSemana,
            h.HoraEntrada,
            h.HoraSalida,
            h.TipoTurno,
            h.HorasPrevistas,
            h.Proyecto != null ? h.Proyecto.Nombre : null))
        .ToListAsync();

    var proyectosDisponibles = await db.Proyectos
        .Where(p => p.Estado == EstadoProyecto.EnCurso || p.Estado == EstadoProyecto.Planificacion)
        .Where(p => p.Tareas.Any(t => t.UsuariosAsignados.Any(u => u.Id == usuarioId)) || p.ResponsableId == usuarioId)
        .OrderBy(p => p.Nombre)
        .Select(p => new ProyectoResumenDto(p.Id, p.Nombre))
        .ToListAsync();

    return Results.Ok(new OperarioFichajeResponse
    {
        NombreUsuario = usuario.NombreCompleto,
        FichajeAbierto = fichajeAbierto,
        Historial = historial,
        HorarioHoy = horarioHoy,
        ProyectosDisponibles = proyectosDisponibles
    });
})
.WithName("GetOperarioFichaje");

app.MapPost("/api/operario/fichaje/entrada", async (
    HttpContext httpContext,
    CrearFichajeRequest request,
    GestionObrasDbContext db) =>
{
    var usuarioId = ObtenerUsuarioAutenticadoId(httpContext.User);
    if (string.IsNullOrWhiteSpace(usuarioId))
    {
        return Results.Unauthorized();
    }

    var ahora = DateTime.Now;
    var hoy = DateOnly.FromDateTime(ahora);

    var abierto = await db.RegistrosFichaje
        .AnyAsync(f => f.UsuarioId == usuarioId && f.Fecha == hoy && f.HoraSalida == null);

    if (abierto)
    {
        return Results.BadRequest(new OperacionFichajeResponse
        {
            Correcto = false,
            Mensaje = "Ya existe una jornada abierta para hoy."
        });
    }

    db.RegistrosFichaje.Add(new RegistroFichaje
    {
        UsuarioId = usuarioId,
        ProyectoId = request.ProyectoId,
        Fecha = hoy,
        HoraEntrada = ahora,
        Estado = EstadoFichaje.Pendiente
    });

    await db.SaveChangesAsync();
    return Results.Ok(new OperacionFichajeResponse { Correcto = true, Mensaje = "Entrada fichada correctamente" });
})
.WithName("PostOperarioFichajeEntrada");

app.MapPost("/api/operario/fichaje/salida", async (
    HttpContext httpContext,
    GestionObrasDbContext db) =>
{
    var usuarioId = ObtenerUsuarioAutenticadoId(httpContext.User);
    if (string.IsNullOrWhiteSpace(usuarioId))
    {
        return Results.Unauthorized();
    }

    var hoy = DateOnly.FromDateTime(DateTime.Today);
    var fichajeAbiertoEntidad = await db.RegistrosFichaje
        .FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.Fecha == hoy && f.HoraSalida == null);

    if (fichajeAbiertoEntidad == null)
    {
        return Results.BadRequest(new OperacionFichajeResponse
        {
            Correcto = false,
            Mensaje = "No existe una jornada abierta para cerrar."
        });
    }

    fichajeAbiertoEntidad.HoraSalida = DateTime.Now;
    await db.SaveChangesAsync();

    return Results.Ok(new OperacionFichajeResponse
    {
        Correcto = true,
        Mensaje = $"Salida fichada. Jornada: {fichajeAbiertoEntidad.TotalHoras:F1} horas"
    });
})
.WithName("PostOperarioFichajeSalida");

app.MapGet("/api/rrhh/dashboard", async (GestionObrasDbContext db) =>
{
    var hoy = DateOnly.FromDateTime(DateTime.Today);
    var limite = hoy.AddDays(30);

    var fichajesHoy = await db.RegistrosFichaje
        .Include(f => f.Usuario)
        .Where(f => f.Fecha == hoy)
        .OrderByDescending(f => f.HoraEntrada)
        .Take(10)
        .Select(f => new RegistroFichajeGestionDto(
            f.Id,
            f.UsuarioId,
            f.Usuario!.NombreCompleto,
            f.Fecha,
            f.HoraEntrada,
            f.HoraSalida,
            f.ProyectoId,
            f.Proyecto != null ? f.Proyecto.Nombre : null,
            f.Estado,
            f.HoraSalida.HasValue ? (f.HoraSalida.Value - f.HoraEntrada).TotalHours : null))
        .ToListAsync();

    var contratosRecientes = await db.Contratos
        .Include(c => c.Usuario)
        .Where(c => c.Activo)
        .OrderByDescending(c => c.FechaCreacion)
        .Take(10)
        .Select(c => new ContratoResumenDto(
            c.Id, c.UsuarioId, c.Usuario!.NombreCompleto, c.TipoContrato, c.Jornada, c.HorasSemanales,
            c.SalarioBrutoAnual, c.FechaInicio, c.FechaFin, c.CentroTrabajo, c.CategoriaConvenio,
            c.NumeroSeguridadSocial, c.Observaciones, c.Activo, c.FechaCreacion, c.FechaModificacion))
        .ToListAsync();

    var contratosProximosVencer = await db.Contratos
        .Include(c => c.Usuario)
        .Where(c => c.Activo && c.FechaFin != null && c.FechaFin <= limite && c.FechaFin >= hoy)
        .OrderBy(c => c.FechaFin)
        .Select(c => new ContratoResumenDto(
            c.Id, c.UsuarioId, c.Usuario!.NombreCompleto, c.TipoContrato, c.Jornada, c.HorasSemanales,
            c.SalarioBrutoAnual, c.FechaInicio, c.FechaFin, c.CentroTrabajo, c.CategoriaConvenio,
            c.NumeroSeguridadSocial, c.Observaciones, c.Activo, c.FechaCreacion, c.FechaModificacion))
        .ToListAsync();

    return Results.Ok(new RRHHDashboardResponse
    {
        TotalTrabajadores = await db.Users.CountAsync(u => u.Activo),
        ContratosActivos = await db.Contratos.CountAsync(c => c.Activo),
        FichajesPendientes = await db.RegistrosFichaje.CountAsync(f => f.Estado == EstadoFichaje.Pendiente && f.HoraSalida != null),
        EnJornadaAhora = await db.RegistrosFichaje.CountAsync(f => f.Fecha == hoy && f.HoraSalida == null),
        FichajesHoy = fichajesHoy,
        ContratosRecientes = contratosRecientes,
        ContratosProximosVencer = contratosProximosVencer
    });
})
.WithName("GetRRHHDashboard");

app.MapGet("/api/rrhh/fichajes", async (
    DateOnly desde,
    DateOnly hasta,
    int? proyectoId,
    string? usuarioId,
    GestionObrasDbContext db) =>
{
    var proyectos = await db.Proyectos
        .OrderBy(p => p.Nombre)
        .Select(p => new ProyectoResumenDto(p.Id, p.Nombre))
        .ToListAsync();

    var trabajadores = await db.Users
        .Where(u => u.Activo)
        .OrderBy(u => u.NombreCompleto)
        .Select(u => new UsuarioResumenDto(u.Id, u.NombreCompleto, u.Cargo))
        .ToListAsync();

    var query = db.RegistrosFichaje
        .Include(f => f.Usuario)
        .Include(f => f.Proyecto)
        .Where(f => f.Fecha >= desde && f.Fecha <= hasta);

    if (proyectoId.HasValue)
    {
        query = query.Where(f => f.ProyectoId == proyectoId.Value);
    }

    if (!string.IsNullOrWhiteSpace(usuarioId))
    {
        query = query.Where(f => f.UsuarioId == usuarioId);
    }

    var fichajes = await query
        .OrderByDescending(f => f.Fecha)
        .ThenByDescending(f => f.HoraEntrada)
        .Select(f => new RegistroFichajeGestionDto(
            f.Id,
            f.UsuarioId,
            f.Usuario!.NombreCompleto,
            f.Fecha,
            f.HoraEntrada,
            f.HoraSalida,
            f.ProyectoId,
            f.Proyecto != null ? f.Proyecto.Nombre : null,
            f.Estado,
            f.HoraSalida.HasValue ? (f.HoraSalida.Value - f.HoraEntrada).TotalHours : null))
        .ToListAsync();

    return Results.Ok(new RRHHFichajesResponse
    {
        Proyectos = proyectos,
        Trabajadores = trabajadores,
        Fichajes = fichajes
    });
})
.WithName("GetRRHHFichajes");

app.MapPost("/api/rrhh/fichajes/{id:int}/estado/{estado}", async (
    int id,
    EstadoFichaje estado,
    GestionObrasDbContext db) =>
{
    var fichaje = await db.RegistrosFichaje.FindAsync(id);
    if (fichaje == null)
    {
        return Results.NotFound(new OperacionResponse { Correcto = false, Mensaje = "Fichaje no encontrado." });
    }

    fichaje.Estado = estado;
    await db.SaveChangesAsync();
    return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = $"Fichaje {(estado == EstadoFichaje.Validado ? "validado" : "rechazado")}" });
})
.WithName("PostRRHHFichajeEstado");

app.MapPost("/api/rrhh/fichajes/validar-pendientes", async (
    DateOnly desde,
    DateOnly hasta,
    int? proyectoId,
    string? usuarioId,
    GestionObrasDbContext db) =>
{
    var query = db.RegistrosFichaje
        .Where(f => f.Fecha >= desde && f.Fecha <= hasta)
        .Where(f => f.Estado == EstadoFichaje.Pendiente && f.HoraSalida != null);

    if (proyectoId.HasValue)
    {
        query = query.Where(f => f.ProyectoId == proyectoId.Value);
    }

    if (!string.IsNullOrWhiteSpace(usuarioId))
    {
        query = query.Where(f => f.UsuarioId == usuarioId);
    }

    var pendientes = await query.ToListAsync();
    foreach (var fichaje in pendientes)
    {
        fichaje.Estado = EstadoFichaje.Validado;
    }

    await db.SaveChangesAsync();
    return Results.Ok(new OperacionResponse
    {
        Correcto = true,
        Mensaje = pendientes.Any()
            ? $"{pendientes.Count} fichajes validados correctamente"
            : "No hay fichajes pendientes para validar"
    });
})
.WithName("PostRRHHValidarPendientes");

app.MapGet("/api/rrhh/contratos", async (GestionObrasDbContext db) =>
{
    var trabajadores = await db.Users
        .Where(u => u.Activo)
        .OrderBy(u => u.NombreCompleto)
        .Select(u => new UsuarioResumenDto(u.Id, u.NombreCompleto, u.Cargo))
        .ToListAsync();

    var contratos = await db.Contratos
        .Include(c => c.Usuario)
        .OrderByDescending(c => c.FechaCreacion)
        .Select(c => new ContratoResumenDto(
            c.Id, c.UsuarioId, c.Usuario!.NombreCompleto, c.TipoContrato, c.Jornada, c.HorasSemanales,
            c.SalarioBrutoAnual, c.FechaInicio, c.FechaFin, c.CentroTrabajo, c.CategoriaConvenio,
            c.NumeroSeguridadSocial, c.Observaciones, c.Activo, c.FechaCreacion, c.FechaModificacion))
        .ToListAsync();

    var hoy = DateOnly.FromDateTime(DateTime.Today);
    var limite = hoy.AddDays(30);
    var proximos = contratos.Count(c => c.Activo && c.FechaFin.HasValue && c.FechaFin.Value <= limite && c.FechaFin.Value >= hoy);

    return Results.Ok(new RRHHContratosResponse
    {
        Trabajadores = trabajadores,
        Contratos = contratos,
        ContratosProximosVencer = proximos
    });
})
.WithName("GetRRHHContratos");

app.MapPost("/api/rrhh/contratos", async (
    GuardarContratoRequest request,
    GestionObrasDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.UsuarioId))
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = "Selecciona un trabajador" });
    }

    if (request.Id > 0)
    {
        var existente = await db.Contratos.FindAsync(request.Id);
        if (existente == null)
        {
            return Results.NotFound(new OperacionResponse { Correcto = false, Mensaje = "Contrato no encontrado." });
        }

        existente.TipoContrato = request.TipoContrato;
        existente.FechaInicio = request.FechaInicio;
        existente.FechaFin = request.FechaFin;
        existente.SalarioBrutoAnual = request.SalarioBrutoAnual;
        existente.Jornada = request.Jornada;
        existente.HorasSemanales = request.HorasSemanales;
        existente.CentroTrabajo = request.CentroTrabajo;
        existente.CategoriaConvenio = request.CategoriaConvenio;
        existente.NumeroSeguridadSocial = request.NumeroSeguridadSocial;
        existente.Observaciones = request.Observaciones;
        existente.FechaModificacion = DateTime.Now;
    }
    else
    {
        db.Contratos.Add(new Contrato
        {
            UsuarioId = request.UsuarioId,
            TipoContrato = request.TipoContrato,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            SalarioBrutoAnual = request.SalarioBrutoAnual,
            Jornada = request.Jornada,
            HorasSemanales = request.HorasSemanales,
            CentroTrabajo = request.CentroTrabajo,
            CategoriaConvenio = request.CategoriaConvenio,
            NumeroSeguridadSocial = request.NumeroSeguridadSocial,
            Observaciones = request.Observaciones,
            Activo = true,
            FechaCreacion = DateTime.Now
        });
    }

    await db.SaveChangesAsync();
    return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = request.Id > 0 ? "Contrato actualizado" : "Contrato creado correctamente" });
})
.WithName("PostRRHHGuardarContrato");

app.MapPost("/api/rrhh/contratos/{id:int}/finalizar", async (int id, GestionObrasDbContext db) =>
{
    var contrato = await db.Contratos.FindAsync(id);
    if (contrato == null)
    {
        return Results.NotFound(new OperacionResponse { Correcto = false, Mensaje = "Contrato no encontrado." });
    }

    contrato.Activo = false;
    contrato.FechaFin = DateOnly.FromDateTime(DateTime.Today);
    contrato.FechaModificacion = DateTime.Now;
    await db.SaveChangesAsync();

    return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = "Contrato finalizado" });
})
.WithName("PostRRHHFinalizarContrato");

app.MapGet("/api/proyectos", async (HttpContext httpContext, GestionObrasDbContext db) =>
{
    var usuarioId = ObtenerUsuarioAutenticadoId(httpContext.User);
    if (string.IsNullOrWhiteSpace(usuarioId))
    {
        return Results.Unauthorized();
    }

    var rolesUsuarioActual = await (
        from ur in db.UserRoles
        join r in db.Roles on ur.RoleId equals r.Id
        where ur.UserId == usuarioId
        select r.Name!)
        .ToListAsync();

    var esAdmin = rolesUsuarioActual.Contains("Administrador");

    var query = db.Proyectos
        .Include(p => p.Responsable)
        .Include(p => p.Tareas)
            .ThenInclude(t => t.UsuariosAsignados)
        .AsQueryable();

    if (!esAdmin)
    {
        if (rolesUsuarioActual.Contains("JefeObra") || rolesUsuarioActual.Contains("OficinaTecnica"))
        {
            query = query.Where(p => p.ResponsableId == usuarioId);
        }
        else
        {
            query = query.Where(p => p.Tareas.Any(t => t.UsuariosAsignados.Any(u => u.Id == usuarioId)));
        }
    }

    var proyectos = await query
        .OrderByDescending(p => p.FechaInicio)
        .Select(p => new ProyectoResumenGestionDto(
            p.Id,
            p.Nombre,
            p.Provincia,
            p.Municipio,
            p.TipoSuelo,
            p.ZonaClimatica,
            p.FechaInicio,
            p.FechaFin,
            p.Estado,
            p.ResponsableId,
            p.Responsable != null ? p.Responsable.NombreCompleto : null,
            p.Tareas.Count,
            p.Tareas.Any(t => t.UsuariosAsignados.Any(u => u.Id == usuarioId))))
        .ToListAsync();

    var rolesResponsables = new[] { "OficinaTecnica", "JefeObra" };
    var responsables = await (
        from u in db.Users
        join ur in db.UserRoles on u.Id equals ur.UserId
        join r in db.Roles on ur.RoleId equals r.Id
        where rolesResponsables.Contains(r.Name!)
        orderby u.NombreCompleto
        select new UsuarioResumenDto(u.Id, u.NombreCompleto, u.Cargo))
        .Distinct()
        .ToListAsync();

    return Results.Ok(new ProyectosResponse
    {
        Proyectos = proyectos,
        ResponsablesDisponibles = responsables,
        EsAdministrador = esAdmin,
        RolesUsuarioActual = rolesUsuarioActual
    });
})
.WithName("GetProyectosPorUsuario");

app.MapPost("/api/proyectos", async (GuardarProyectoRequest request, GestionObrasDbContext db) =>
{
    if (request.Id > 0)
    {
        var existente = await db.Proyectos.FindAsync(request.Id);
        if (existente == null)
        {
            return Results.NotFound(new OperacionResponse { Correcto = false, Mensaje = "Proyecto no encontrado." });
        }

        existente.Nombre = request.Nombre;
        existente.Provincia = request.Provincia;
        existente.Municipio = request.Municipio;
        existente.TipoSuelo = request.TipoSuelo;
        existente.ZonaClimatica = request.ZonaClimatica;
        existente.FechaInicio = request.FechaInicio;
        existente.FechaFin = request.FechaFin;
        existente.Estado = request.Estado;
        existente.ResponsableId = request.ResponsableId;
    }
    else
    {
        db.Proyectos.Add(new Proyecto
        {
            Nombre = request.Nombre,
            Provincia = request.Provincia,
            Municipio = request.Municipio,
            TipoSuelo = request.TipoSuelo,
            ZonaClimatica = request.ZonaClimatica,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            Estado = request.Estado,
            ResponsableId = request.ResponsableId
        });
    }

    await db.SaveChangesAsync();
    return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = request.Id > 0 ? "Proyecto actualizado" : "Proyecto creado correctamente" });
})
.WithName("PostGuardarProyecto");

app.MapDelete("/api/proyectos/{id:int}", async (int id, GestionObrasDbContext db) =>
{
    var proyecto = await db.Proyectos.FindAsync(id);
    if (proyecto == null)
    {
        return Results.NotFound(new OperacionResponse { Correcto = false, Mensaje = "Proyecto no encontrado." });
    }

    db.Proyectos.Remove(proyecto);
    await db.SaveChangesAsync();
    return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = "Proyecto eliminado" });
})
.WithName("DeleteProyecto");

app.MapGet("/api/materiales/gestion", async (GestionObrasDbContext db) =>
{
    var materiales = await db.Materiales
        .Include(m => m.Proveedor)
        .Include(m => m.Proveedores)
        .OrderBy(m => m.Nombre)
        .ToListAsync();

    var proveedores = await db.Proveedores
        .OrderBy(p => p.Nombre)
        .ToListAsync();

    var categorias = await db.CategoriasMateriales
        .Where(c => c.Activa)
        .OrderBy(c => c.Nombre)
        .ToListAsync();

    return Results.Ok(new MaterialesGestionResponse
    {
        Materiales = materiales.Select(MapMaterial).ToList(),
        Proveedores = proveedores.Select(MapProveedor).ToList(),
        Categorias = categorias.Select(MapCategoria).ToList()
    });
})
.WithName("GetMaterialesGestion");

app.MapPost("/api/materiales", async (GuardarMaterialRequest request, GestionObrasDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Codigo) ||
        string.IsNullOrWhiteSpace(request.Nombre) ||
        string.IsNullOrWhiteSpace(request.Categoria) ||
        string.IsNullOrWhiteSpace(request.UnidadMedida))
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = "Codigo, nombre, categoria y unidad son obligatorios." });
    }

    var proveedorIds = request.ProveedorIds.Distinct().ToList();
    var proveedores = proveedorIds.Count == 0
        ? new List<Proveedor>()
        : await db.Proveedores.Where(p => proveedorIds.Contains(p.Id)).ToListAsync();

    if (request.Id > 0)
    {
        var existente = await db.Materiales
            .Include(m => m.Proveedores)
            .FirstOrDefaultAsync(m => m.Id == request.Id);

        if (existente == null)
        {
            return Results.NotFound(new OperacionResponse { Correcto = false, Mensaje = "Material no encontrado." });
        }

        existente.Codigo = request.Codigo.Trim();
        existente.Nombre = request.Nombre.Trim();
        existente.Descripcion = request.Descripcion.Trim();
        existente.Categoria = request.Categoria.Trim();
        existente.UnidadMedida = request.UnidadMedida.Trim();
        existente.PrecioUnitario = request.PrecioUnitario;
        existente.StockDisponible = request.StockDisponible;
        existente.StockMinimo = request.StockMinimo;
        existente.Activo = request.Activo;
        existente.ProveedorId = proveedores.FirstOrDefault()?.Id;
        existente.Proveedores.Clear();

        foreach (var proveedor in proveedores)
        {
            existente.Proveedores.Add(proveedor);
        }
    }
    else
    {
        var material = new Material
        {
            Codigo = request.Codigo.Trim(),
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion.Trim(),
            Categoria = request.Categoria.Trim(),
            UnidadMedida = request.UnidadMedida.Trim(),
            PrecioUnitario = request.PrecioUnitario,
            StockDisponible = request.StockDisponible,
            StockMinimo = request.StockMinimo,
            Activo = request.Activo,
            ProveedorId = proveedores.FirstOrDefault()?.Id,
            Proveedores = proveedores
        };

        db.Materiales.Add(material);
    }

    await db.SaveChangesAsync();
    return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = request.Id > 0 ? "Material actualizado." : "Material creado correctamente." });
})
.WithName("PostGuardarMaterial");

app.MapDelete("/api/materiales/{id:int}", async (int id, GestionObrasDbContext db) =>
{
    var material = await db.Materiales.FindAsync(id);
    if (material == null)
    {
        return Results.NotFound(new OperacionResponse { Correcto = false, Mensaje = "Material no encontrado." });
    }

    db.Materiales.Remove(material);
    await db.SaveChangesAsync();
    return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = "Material eliminado." });
})
.WithName("DeleteMaterial");

app.MapPost("/api/materiales/{id:int}/alternar", async (int id, GestionObrasDbContext db) =>
{
    var material = await db.Materiales.FindAsync(id);
    if (material == null)
    {
        return Results.NotFound(new OperacionResponse { Correcto = false, Mensaje = "Material no encontrado." });
    }

    material.Activo = !material.Activo;
    await db.SaveChangesAsync();
    return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = $"Material {(material.Activo ? "activado" : "desactivado")}." });
})
.WithName("PostAlternarMaterial");

app.MapGet("/api/materiales/catalogos", async (GestionObrasDbContext db) =>
{
    var proveedores = await db.Proveedores.OrderBy(p => p.Nombre).ToListAsync();
    var materiales = await db.Materiales
        .Include(m => m.Proveedor)
        .Include(m => m.Proveedores)
        .OrderBy(m => m.Nombre)
        .ToListAsync();
    var categorias = await db.CategoriasMateriales.OrderBy(c => c.Nombre).ToListAsync();

    return Results.Ok(new CatalogosMaterialesResponse
    {
        Proveedores = proveedores.Select(MapProveedor).ToList(),
        Materiales = materiales.Select(MapMaterial).ToList(),
        Categorias = categorias.Select(MapCategoria).ToList()
    });
})
.WithName("GetCatalogosMateriales");

app.MapPost("/api/materiales/proveedores", async (GuardarProveedorRequest request, GestionObrasDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.CIF))
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = "Nombre y CIF son obligatorios." });
    }

    if (request.Id > 0)
    {
        var existente = await db.Proveedores.FindAsync(request.Id);
        if (existente == null)
        {
            return Results.NotFound(new OperacionResponse { Correcto = false, Mensaje = "Proveedor no encontrado." });
        }

        existente.Nombre = request.Nombre.Trim();
        existente.CIF = request.CIF.Trim();
        existente.Direccion = request.Direccion.Trim();
        existente.Telefono = request.Telefono.Trim();
        existente.Email = request.Email.Trim();
        existente.Activo = request.Activo;
    }
    else
    {
        db.Proveedores.Add(new Proveedor
        {
            Nombre = request.Nombre.Trim(),
            CIF = request.CIF.Trim(),
            Direccion = request.Direccion.Trim(),
            Telefono = request.Telefono.Trim(),
            Email = request.Email.Trim(),
            Activo = request.Activo
        });
    }

    await db.SaveChangesAsync();
    return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = request.Id > 0 ? "Proveedor actualizado." : "Proveedor creado correctamente." });
})
.WithName("PostGuardarProveedor");

app.MapPost("/api/materiales/categorias", async (GuardarCategoriaMaterialRequest request, GestionObrasDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Nombre))
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = "La categoria necesita un nombre." });
    }

    if (request.Id > 0)
    {
        var existente = await db.CategoriasMateriales.FindAsync(request.Id);
        if (existente == null)
        {
            return Results.NotFound(new OperacionResponse { Correcto = false, Mensaje = "Categoria no encontrada." });
        }

        existente.Nombre = request.Nombre.Trim();
        existente.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();
        existente.Activa = request.Activa;
    }
    else
    {
        db.CategoriasMateriales.Add(new CategoriaMaterial
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim(),
            Activa = request.Activa
        });
    }

    await db.SaveChangesAsync();
    return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = request.Id > 0 ? "Categoria actualizada." : "Categoria creada correctamente." });
})
.WithName("PostGuardarCategoriaMaterial");

app.MapPost("/api/materiales/proveedores/{id:int}/alternar", async (int id, GestionObrasDbContext db) =>
{
    var proveedor = await db.Proveedores.FindAsync(id);
    if (proveedor == null)
    {
        return Results.NotFound(new OperacionResponse { Correcto = false, Mensaje = "Proveedor no encontrado." });
    }

    proveedor.Activo = !proveedor.Activo;
    if (!proveedor.Activo)
    {
        var materialesRelacionados = await db.Materiales
            .Include(m => m.Proveedores)
            .Where(m => m.ProveedorId == proveedor.Id || m.Proveedores.Any(p => p.Id == proveedor.Id))
            .ToListAsync();

        foreach (var material in materialesRelacionados)
        {
            material.Activo = false;
        }
    }

    await db.SaveChangesAsync();
    return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = $"Proveedor {(proveedor.Activo ? "activado" : "desactivado")}." });
})
.WithName("PostAlternarProveedor");

app.MapPost("/api/materiales/categorias/{id:int}/alternar", async (int id, GestionObrasDbContext db) =>
{
    var categoria = await db.CategoriasMateriales.FindAsync(id);
    if (categoria == null)
    {
        return Results.NotFound(new OperacionResponse { Correcto = false, Mensaje = "Categoria no encontrada." });
    }

    categoria.Activa = !categoria.Activa;
    await db.SaveChangesAsync();
    return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = $"Categoria {(categoria.Activa ? "activada" : "desactivada")}." });
})
.WithName("PostAlternarCategoriaMaterial");

app.MapGet("/api/materiales/solicitudes/jefe-obra", async (HttpContext httpContext, GestionObrasDbContext db) =>
{
    var usuarioId = ObtenerUsuarioAutenticadoId(httpContext.User);
    if (string.IsNullOrWhiteSpace(usuarioId))
    {
        return Results.Unauthorized();
    }

    var misProyectos = await db.Proyectos
        .Where(p => p.ResponsableId == usuarioId && p.Estado != EstadoProyecto.Finalizado)
        .OrderBy(p => p.Nombre)
        .Select(p => new ProyectoResumenDto(p.Id, p.Nombre))
        .ToListAsync();

    var materiales = await db.Materiales
        .Include(m => m.Proveedor)
        .Include(m => m.Proveedores)
        .Where(m => m.Activo)
        .OrderBy(m => m.Nombre)
        .ToListAsync();

    return Results.Ok(new SolicitarMaterialesResponse
    {
        MisProyectos = misProyectos,
        Materiales = materiales.Select(MapMaterial).ToList()
    });
})
.WithName("GetSolicitarMateriales");

app.MapPost("/api/materiales/solicitudes", async (HttpContext httpContext, CrearSolicitudMaterialRequest request, GestionObrasDbContext db) =>
{
    var usuarioId = ObtenerUsuarioAutenticadoId(httpContext.User);
    if (string.IsNullOrWhiteSpace(usuarioId))
    {
        return Results.Unauthorized();
    }

    if (request.CantidadSolicitada <= 0)
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = "La cantidad debe ser mayor que 0." });
    }

    if (string.IsNullOrWhiteSpace(request.Justificacion))
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = "Debe proporcionar una justificacion." });
    }

    var proyectoValido = await db.Proyectos.AnyAsync(p => p.Id == request.ProyectoId && p.ResponsableId == usuarioId);
    if (!proyectoValido)
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = "El proyecto seleccionado no pertenece al usuario." });
    }

    db.SolicitudesMateriales.Add(new SolicitudMaterial
    {
        MaterialId = request.MaterialId,
        ProyectoId = request.ProyectoId,
        CantidadSolicitada = request.CantidadSolicitada,
        Justificacion = request.Justificacion.Trim(),
        SolicitadoPorId = usuarioId,
        FechaSolicitud = DateTime.Now,
        Estado = EstadoSolicitudMaterial.Pendiente,
        Prioridad = request.Prioridad,
        FechaNecesaria = request.FechaNecesaria
    });

    await db.SaveChangesAsync();
    return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = "Solicitud enviada correctamente." });
})
.WithName("PostCrearSolicitudMaterial");

app.MapGet("/api/materiales/solicitudes/jefe-obra/historial", async (HttpContext httpContext, GestionObrasDbContext db) =>
{
    var usuarioId = ObtenerUsuarioAutenticadoId(httpContext.User);
    if (string.IsNullOrWhiteSpace(usuarioId))
    {
        return Results.Unauthorized();
    }

    var solicitudes = await db.SolicitudesMateriales
        .Include(s => s.Material).ThenInclude(m => m.Proveedor)
        .Include(s => s.Material).ThenInclude(m => m.Proveedores)
        .Include(s => s.Proyecto)
        .Include(s => s.SolicitadoPor)
        .Include(s => s.RevisadoPor)
        .Where(s => s.SolicitadoPorId == usuarioId)
        .OrderByDescending(s => s.FechaSolicitud)
        .ToListAsync();

    var proyectos = await db.Proyectos
        .Where(p => p.ResponsableId == usuarioId)
        .OrderBy(p => p.Nombre)
        .Select(p => new ProyectoResumenDto(p.Id, p.Nombre))
        .ToListAsync();

    return Results.Ok(new MisSolicitudesMaterialesResponse
    {
        Solicitudes = solicitudes.Select(MapSolicitud).ToList(),
        Proyectos = proyectos
    });
})
.WithName("GetMisSolicitudesMateriales");

app.MapPost("/api/materiales/solicitudes/{id:int}/cancelar", async (int id, GestionObrasDbContext db) =>
{
    var solicitud = await db.SolicitudesMateriales.FindAsync(id);
    if (solicitud == null)
    {
        return Results.NotFound(new OperacionResponse { Correcto = false, Mensaje = "Solicitud no encontrada." });
    }

    if (solicitud.Estado != EstadoSolicitudMaterial.Pendiente)
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = "Solo se pueden cancelar solicitudes pendientes." });
    }

    solicitud.Estado = EstadoSolicitudMaterial.Cancelada;
    await db.SaveChangesAsync();
    return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = "Solicitud cancelada." });
})
.WithName("PostCancelarSolicitudMaterial");

app.MapGet("/api/materiales/solicitudes/admin", async (GestionObrasDbContext db) =>
{
    var solicitudes = await db.SolicitudesMateriales
        .Include(s => s.Material).ThenInclude(m => m.Proveedor)
        .Include(s => s.Material).ThenInclude(m => m.Proveedores)
        .Include(s => s.Proyecto)
        .Include(s => s.SolicitadoPor)
        .Include(s => s.RevisadoPor)
        .OrderByDescending(s => s.FechaSolicitud)
        .ToListAsync();

    var proyectos = await db.Proyectos
        .OrderBy(p => p.Nombre)
        .Select(p => new ProyectoResumenDto(p.Id, p.Nombre))
        .ToListAsync();

    return Results.Ok(new GestionSolicitudesMaterialesResponse
    {
        Solicitudes = solicitudes.Select(MapSolicitud).ToList(),
        Proyectos = proyectos
    });
})
.WithName("GetGestionSolicitudesMateriales");

app.MapPost("/api/materiales/solicitudes/{id:int}/revisar", async (int id, RevisarSolicitudMaterialRequest request, GestionObrasDbContext db) =>
{
    var solicitud = await db.SolicitudesMateriales
        .Include(s => s.Material)
        .FirstOrDefaultAsync(s => s.Id == id);

    if (solicitud == null)
    {
        return Results.NotFound(new OperacionResponse { Correcto = false, Mensaje = "Solicitud no encontrada." });
    }

    if (solicitud.Estado != EstadoSolicitudMaterial.Pendiente)
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = "La solicitud ya fue revisada." });
    }

    solicitud.RevisadoPorId = request.RevisadoPorId;
    solicitud.FechaRespuesta = DateTime.Now;
    solicitud.ObservacionesAdmin = string.IsNullOrWhiteSpace(request.ObservacionesAdmin) ? null : request.ObservacionesAdmin.Trim();

    if (request.Aprobar)
    {
        if (solicitud.Material.StockDisponible < solicitud.CantidadSolicitada)
        {
            return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = "Stock insuficiente para aprobar la solicitud." });
        }

        solicitud.Estado = EstadoSolicitudMaterial.Aprobada;
        solicitud.Material.StockDisponible -= (int)solicitud.CantidadSolicitada;
        await db.SaveChangesAsync();
        return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = "Solicitud aprobada correctamente." });
    }

    if (string.IsNullOrWhiteSpace(request.ObservacionesAdmin))
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = "Debe proporcionar un motivo para el rechazo." });
    }

    solicitud.Estado = EstadoSolicitudMaterial.Rechazada;
    await db.SaveChangesAsync();
    return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = "Solicitud rechazada." });
})
.WithName("PostRevisarSolicitudMaterial");

app.MapGet("/api/consultas/dashboard", async (GestionObrasDbContext db) =>
{
    var proyectosRecientes = await db.Proyectos
        .OrderByDescending(p => p.FechaInicio)
        .Take(5)
        .Select(p => new ProyectoConsultaDto(
            p.Id,
            p.Nombre,
            p.Municipio,
            p.Provincia,
            p.Estado,
            p.FechaInicio,
            p.Tareas.Count,
            p.Presupuesto != null))
        .ToListAsync();

    var totalProyectos = await db.Proyectos.CountAsync();
    var proyectosEnCurso = await db.Proyectos.CountAsync(p => p.Estado == EstadoProyecto.EnCurso);
    var totalTareas = await db.Tareas.CountAsync(t => t.Estado != EstadoTarea.Finalizado);
    var tareasBloqueadas = await db.Tareas.CountAsync(t => t.Estado == EstadoTarea.Bloqueado);
    var totalEmpleados = await db.Empleados.CountAsync();
    var totalMateriales = await db.Materiales.CountAsync();

    var alertas = new List<string>();
    if (tareasBloqueadas > 0)
    {
        alertas.Add($"{tareasBloqueadas} tareas bloqueadas requieren atencion");
    }

    if (totalProyectos == 0)
    {
        alertas.Add("No hay proyectos registrados. Comienza creando tu primer proyecto.");
    }

    var proyectosBloqueados = proyectosRecientes.Count(p => p.Estado == EstadoProyecto.Bloqueado);
    if (proyectosBloqueados > 0)
    {
        alertas.Add($"{proyectosBloqueados} proyectos estan en estado bloqueado");
    }

    return Results.Ok(new DashboardGeneralResponse
    {
        TotalProyectos = totalProyectos,
        ProyectosEnCurso = proyectosEnCurso,
        TotalTareas = totalTareas,
        TareasBloqueadas = tareasBloqueadas,
        TotalEmpleados = totalEmpleados,
        TotalMateriales = totalMateriales,
        Alertas = alertas,
        ProyectosRecientes = proyectosRecientes
    });
})
.WithName("GetDashboardGeneral");

app.MapGet("/api/consultas/admin/dashboard", async (GestionObrasDbContext db) =>
{
    var proyectosRecientes = await db.Proyectos
        .OrderByDescending(p => p.FechaInicio)
        .Take(5)
        .Select(p => new ProyectoConsultaDto(
            p.Id,
            p.Nombre,
            p.Municipio,
            p.Provincia,
            p.Estado,
            p.FechaInicio,
            p.Tareas.Count,
            p.Presupuesto != null))
        .ToListAsync();

    return Results.Ok(new AdminDashboardResponse
    {
        TotalProyectos = await db.Proyectos.CountAsync(),
        TotalUsuarios = await db.Users.CountAsync(),
        TotalEmpleados = await db.Empleados.CountAsync(),
        TotalFacturas = await db.Facturas.CountAsync(),
        ProyectosRecientes = proyectosRecientes
    });
})
.WithName("GetAdminDashboard");

app.MapGet("/api/consultas/jefe-obra/dashboard", async (HttpContext httpContext, GestionObrasDbContext db) =>
{
    var usuarioId = ObtenerUsuarioAutenticadoId(httpContext.User);
    if (string.IsNullOrWhiteSpace(usuarioId))
    {
        return Results.Unauthorized();
    }

    var misProyectos = await db.Proyectos
        .Include(p => p.Tareas)
        .Where(p => p.ResponsableId == usuarioId)
        .OrderByDescending(p => p.FechaInicio)
        .Take(10)
        .ToListAsync();

    var proyectoIds = misProyectos.Select(p => p.Id).ToList();

    return Results.Ok(new JefeObraDashboardResponse
    {
        ProyectosActivos = misProyectos.Count(p => p.Estado == EstadoProyecto.EnCurso),
        TareasPendientes = await db.Tareas.CountAsync(t => proyectoIds.Contains(t.ProyectoId) && t.Estado == EstadoTarea.Pendiente),
        TareasBloqueadas = await db.Tareas.CountAsync(t => proyectoIds.Contains(t.ProyectoId) && t.Estado == EstadoTarea.Bloqueado),
        TotalMateriales = await db.Materiales.CountAsync(),
        MisProyectos = misProyectos.Select(MapProyectoConsulta).ToList()
    });
})
.WithName("GetJefeObraDashboard");

app.MapGet("/api/consultas/oficina-tecnica/dashboard", async (GestionObrasDbContext db) =>
{
    var proyectosPlanificacion = await db.Proyectos
        .Include(p => p.Presupuesto)
        .Where(p => p.Estado == EstadoProyecto.Planificacion)
        .OrderBy(p => p.FechaInicio)
        .Take(10)
        .ToListAsync();

    return Results.Ok(new OficinaTecnicaDashboardResponse
    {
        TotalCarpetas = await db.CarpetasLegales.CountAsync(),
        TotalPresupuestos = await db.Presupuestos.CountAsync(),
        ProyectosPlanificacion = await db.Proyectos.CountAsync(p => p.Estado == EstadoProyecto.Planificacion),
        TotalFacturas = await db.Facturas.CountAsync(),
        ProyectosPlanificacionLista = proyectosPlanificacion.Select(MapProyectoConsulta).ToList()
    });
})
.WithName("GetOficinaTecnicaDashboard");

app.MapGet("/api/consultas/proyectos/{proyectoId:int}/gantt", async (int proyectoId, GestionObrasDbContext db) =>
{
    var proyecto = await db.Proyectos
        .Where(p => p.Id == proyectoId)
        .Select(p => new ProyectoGanttDto(p.Id, p.Nombre))
        .FirstOrDefaultAsync();

    if (proyecto == null)
    {
        return Results.NotFound();
    }

    var tareas = await db.Tareas
        .Where(t => t.ProyectoId == proyectoId)
        .OrderBy(t => t.FechaInicio)
        .Select(t => new TareaGanttDto(
            t.Id,
            t.Nombre,
            t.Estado,
            t.FechaInicio,
            t.FechaFin,
            t.Nivel,
            t.Prioridad))
        .ToListAsync();

    return Results.Ok(new GanttProyectoResponse
    {
        Proyecto = proyecto,
        Tareas = tareas
    });
})
.WithName("GetGanttProyecto");

app.MapGet("/api/consultas/proyectos/{proyectoId:int}/historial-materiales", async (int proyectoId, GestionObrasDbContext db) =>
{
    var proyecto = await db.Proyectos
        .Where(p => p.Id == proyectoId)
        .Select(p => new ProyectoGanttDto(p.Id, p.Nombre))
        .FirstOrDefaultAsync();

    if (proyecto == null)
    {
        return Results.NotFound();
    }

    var solicitudes = await db.SolicitudesMateriales
        .Include(s => s.Material).ThenInclude(m => m.Proveedor)
        .Include(s => s.Material).ThenInclude(m => m.Proveedores)
        .Include(s => s.Proyecto)
        .Include(s => s.SolicitadoPor)
        .Include(s => s.RevisadoPor)
        .Where(s => s.ProyectoId == proyectoId)
        .OrderByDescending(s => s.FechaSolicitud)
        .ToListAsync();

    return Results.Ok(new HistorialMaterialesProyectoResponse
    {
        Proyecto = proyecto,
        Solicitudes = solicitudes.Select(MapSolicitud).ToList()
    });
})
.WithName("GetHistorialMaterialesProyecto");

app.MapGet("/api/administracion/usuarios", async (GestionObrasDbContext db, UserManager<UsuarioObra> userManager) =>
{
    var usuarios = await db.Users.OrderBy(u => u.UserName).ToListAsync();
    var resultado = new List<UsuarioGestionDto>();

    foreach (var usuario in usuarios)
    {
        var roles = await userManager.GetRolesAsync(usuario);
        resultado.Add(new UsuarioGestionDto
        {
            Id = usuario.Id,
            UserName = usuario.UserName ?? string.Empty,
            NombreCompleto = usuario.NombreCompleto,
            Email = usuario.Email,
            DNI = usuario.DNI,
            EmailConfirmed = usuario.EmailConfirmed,
            Activo = usuario.Activo,
            Roles = roles.ToList()
        });
    }

    return Results.Ok(new GestionUsuariosResponse { Usuarios = resultado });
})
.WithName("GetAdministracionUsuarios");

app.MapPost("/api/administracion/usuarios", async (
    GuardarUsuarioAdminRequest request,
    UserManager<UsuarioObra> userManager) =>
{
    if (string.IsNullOrWhiteSpace(request.NombreCompleto) ||
        string.IsNullOrWhiteSpace(request.DNI) ||
        string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.UserName) ||
        string.IsNullOrWhiteSpace(request.Rol))
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = "Todos los campos obligatorios deben informarse." });
    }

    if (string.IsNullOrWhiteSpace(request.Id))
    {
        var nuevoUsuario = new UsuarioObra
        {
            UserName = request.UserName,
            Email = request.Email,
            NombreCompleto = request.NombreCompleto,
            DNI = request.DNI,
            TipoUsuario = UsuarioPerfilRules.MapRolPrincipalATipoUsuario(request.Rol),
            EmailConfirmed = true,
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        var result = await userManager.CreateAsync(nuevoUsuario, request.Password);
        if (!result.Succeeded)
        {
            return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = string.Join(" | ", result.Errors.Select(e => e.Description)) });
        }

        await userManager.AddToRoleAsync(nuevoUsuario, request.Rol);
        return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = "Usuario creado correctamente." });
    }

    var usuarioExistente = await userManager.FindByIdAsync(request.Id);
    if (usuarioExistente == null)
    {
        return Results.NotFound(new OperacionResponse { Correcto = false, Mensaje = "Usuario no encontrado." });
    }

    usuarioExistente.NombreCompleto = request.NombreCompleto;
    usuarioExistente.DNI = request.DNI;
    usuarioExistente.Email = request.Email;
    usuarioExistente.TipoUsuario = UsuarioPerfilRules.MapRolPrincipalATipoUsuario(request.Rol);

    var updateResult = await userManager.UpdateAsync(usuarioExistente);
    if (!updateResult.Succeeded)
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = string.Join(" | ", updateResult.Errors.Select(e => e.Description)) });
    }

    var rolesActuales = await userManager.GetRolesAsync(usuarioExistente);
    if (rolesActuales.Any())
    {
        await userManager.RemoveFromRolesAsync(usuarioExistente, rolesActuales);
    }

    await userManager.AddToRoleAsync(usuarioExistente, request.Rol);
    return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = "Usuario actualizado." });
})
.WithName("PostAdministracionUsuarios");

app.MapDelete("/api/administracion/usuarios/{id}", async (string id, UserManager<UsuarioObra> userManager) =>
{
    var usuario = await userManager.FindByIdAsync(id);
    if (usuario == null)
    {
        return Results.NotFound(new OperacionResponse { Correcto = false, Mensaje = "Usuario no encontrado." });
    }

    var result = await userManager.DeleteAsync(usuario);
    if (!result.Succeeded)
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = string.Join(" | ", result.Errors.Select(e => e.Description)) });
    }

    return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = "Usuario eliminado." });
})
.WithName("DeleteAdministracionUsuario");

app.MapGet("/api/administracion/empleados", async (
    HttpContext httpContext,
    GestionObrasDbContext db,
    UserManager<UsuarioObra> userManager) =>
{
    var usuarioId = ObtenerUsuarioAutenticadoId(httpContext.User);
    if (string.IsNullOrWhiteSpace(usuarioId))
    {
        return Results.Unauthorized();
    }

    var usuarioActual = await userManager.FindByIdAsync(usuarioId);
    if (usuarioActual == null)
    {
        return Results.NotFound();
    }

    var rolesUsuarioActual = (await userManager.GetRolesAsync(usuarioActual)).ToHashSet();
    var descripcionJerarquiaActual = ObtenerDescripcionJerarquia(rolesUsuarioActual);
    var usuarios = await db.Users.OrderBy(u => u.NombreCompleto).ToListAsync();

    var lista = new List<EmpleadoGestionDto>();
    var idSecuencial = 1;

    foreach (var usuario in usuarios)
    {
        var rolesEmpleado = await userManager.GetRolesAsync(usuario);
        if (!PuedeVerUsuarioPorJerarquia(rolesUsuarioActual, rolesEmpleado))
        {
            continue;
        }

        var (nombre, apellidos) = SepararNombreCompleto(usuario.NombreCompleto);
        var rolPrincipal = rolesEmpleado.FirstOrDefault() ?? "Sin rol";

        lista.Add(new EmpleadoGestionDto
        {
            Id = idSecuencial++,
            UsuarioId = usuario.Id,
            Nombre = nombre,
            Apellidos = apellidos,
            DNI = usuario.DNI,
            Email = usuario.Email ?? string.Empty,
            Telefono = usuario.TelefonoMovil ?? usuario.PhoneNumber ?? string.Empty,
            Departamento = rolPrincipal,
            Cargo = string.IsNullOrWhiteSpace(usuario.Cargo) ? rolPrincipal : usuario.Cargo,
            FechaContratacion = usuario.FechaCreacion,
            Direccion = null,
            Activo = usuario.Activo
        });
    }

    return Results.Ok(new GestionEmpleadosResponse
    {
        Empleados = lista,
        DescripcionJerarquiaActual = descripcionJerarquiaActual
    });
})
.WithName("GetAdministracionEmpleados");

app.MapPost("/api/administracion/empleados", async (
    GuardarEmpleadoRequest request,
    UserManager<UsuarioObra> userManager) =>
{
    if (string.IsNullOrWhiteSpace(request.Nombre) ||
        string.IsNullOrWhiteSpace(request.Apellidos) ||
        string.IsNullOrWhiteSpace(request.DNI) ||
        string.IsNullOrWhiteSpace(request.Email))
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = "Faltan campos obligatorios del empleado." });
    }

    if (string.IsNullOrWhiteSpace(request.UsuarioId))
    {
        var nuevoUsuario = new UsuarioObra
        {
            UserName = request.Email,
            Email = request.Email,
            NombreCompleto = $"{request.Nombre} {request.Apellidos}".Trim(),
            DNI = request.DNI,
            TelefonoMovil = request.Telefono,
            Cargo = request.Cargo,
            Activo = request.Activo,
            FechaCreacion = request.FechaContratacion
        };

        var createResult = await userManager.CreateAsync(nuevoUsuario, "Temporal1");
        if (!createResult.Succeeded)
        {
            return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = string.Join(" | ", createResult.Errors.Select(e => e.Description)) });
        }

        if (!string.IsNullOrWhiteSpace(request.Departamento))
        {
            await userManager.AddToRoleAsync(nuevoUsuario, request.Departamento);
        }

        return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = "Empleado creado correctamente." });
    }

    var usuario = await userManager.FindByIdAsync(request.UsuarioId);
    if (usuario == null)
    {
        return Results.NotFound(new OperacionResponse { Correcto = false, Mensaje = "Empleado no encontrado." });
    }

    usuario.NombreCompleto = $"{request.Nombre} {request.Apellidos}".Trim();
    usuario.DNI = request.DNI;
    usuario.Email = request.Email;
    usuario.UserName = request.Email;
    usuario.TelefonoMovil = request.Telefono;
    usuario.Cargo = request.Cargo;
    usuario.Activo = request.Activo;

    var updateEmpleado = await userManager.UpdateAsync(usuario);
    if (!updateEmpleado.Succeeded)
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = string.Join(" | ", updateEmpleado.Errors.Select(e => e.Description)) });
    }

    return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = "Empleado actualizado." });
})
.WithName("PostAdministracionEmpleado");

app.MapDelete("/api/administracion/empleados/{usuarioId}", async (string usuarioId, UserManager<UsuarioObra> userManager) =>
{
    var usuario = await userManager.FindByIdAsync(usuarioId);
    if (usuario == null)
    {
        return Results.NotFound(new OperacionResponse { Correcto = false, Mensaje = "Empleado no encontrado." });
    }

    usuario.Activo = false;
    var result = await userManager.UpdateAsync(usuario);
    if (!result.Succeeded)
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = string.Join(" | ", result.Errors.Select(e => e.Description)) });
    }

    return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = "Empleado desactivado." });
})
.WithName("DeleteAdministracionEmpleado");

app.MapGet("/api/administracion/tablero-proyectos", async (GestionObrasDbContext db) =>
{
    var proyectos = await db.Proyectos
        .Include(p => p.Tareas)
        .Include(p => p.EmpleadosAsignados)
        .OrderByDescending(p => p.FechaInicio)
        .ToListAsync();

    return Results.Ok(new TableroProyectosResponse
    {
        Proyectos = proyectos.Select(p => new ProyectoTableroDto
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Provincia = p.Provincia,
            Municipio = p.Municipio,
            TipoSuelo = p.TipoSuelo,
            ZonaClimatica = p.ZonaClimatica,
            FechaInicio = p.FechaInicio,
            FechaFin = p.FechaFin,
            Estado = p.Estado,
            TotalTareas = p.Tareas.Count,
            TareasPendientes = p.Tareas.Count(t => t.Estado == EstadoTarea.Pendiente),
            TareasEnCurso = p.Tareas.Count(t => t.Estado == EstadoTarea.EnCurso),
            TareasFinalizadas = p.Tareas.Count(t => t.Estado == EstadoTarea.Finalizado),
            TareasBloqueadas = p.Tareas.Count(t => t.Estado == EstadoTarea.Bloqueado),
            EmpleadosAsignados = p.EmpleadosAsignados.Count
        }).ToList()
    });
})
.WithName("GetAdministracionTableroProyectos");

app.MapPost("/api/administracion/tablero-proyectos/{id:int}/estado/{estado}", async (int id, EstadoProyecto estado, GestionObrasDbContext db) =>
{
    var proyecto = await db.Proyectos.FindAsync(id);
    if (proyecto == null)
    {
        return Results.NotFound(new OperacionResponse { Correcto = false, Mensaje = "Proyecto no encontrado." });
    }

    proyecto.Estado = estado;
    if (estado == EstadoProyecto.Finalizado && proyecto.FechaFin == null)
    {
        proyecto.FechaFin = DateTime.Today;
    }

    await db.SaveChangesAsync();
    return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = $"Proyecto actualizado a {estado}." });
})
.WithName("PostAdministracionCambiarEstadoProyecto");

app.MapGet("/api/administracion/mi-tablero", async (
    HttpContext httpContext,
    GestionObrasDbContext db,
    UserManager<UsuarioObra> userManager) =>
{
    var usuarioId = ObtenerUsuarioAutenticadoId(httpContext.User);
    if (string.IsNullOrWhiteSpace(usuarioId))
    {
        return Results.Unauthorized();
    }

    var usuarioActual = await userManager.FindByIdAsync(usuarioId);
    if (usuarioActual == null)
    {
        return Results.NotFound();
    }

    var roles = await userManager.GetRolesAsync(usuarioActual);
    var rolesOperario = new[] { "Operario", "OperarioObra", "OperarioOficinaT" };
    var esOperario = roles.Any(r => rolesOperario.Contains(r));

    var consultaTareas = db.Tareas
        .Include(t => t.Proyecto)
        .Include(t => t.UsuariosAsignados)
        .Include(t => t.Bloqueo)
        .Include(t => t.TareaPadre)
        .Where(t => t.UsuariosAsignados.Any(r => r.Id == usuarioId));

    if (esOperario)
    {
        consultaTareas = consultaTareas.Where(t => !db.Tareas.Any(sub => sub.TareaPadreId == t.Id));
    }

    var tareas = await consultaTareas
        .OrderBy(t => t.Prioridad)
        .ThenBy(t => t.FechaInicio)
        .ToListAsync();

    var proyectos = tareas
        .Select(t => t.Proyecto)
        .DistinctBy(p => p.Id)
        .OrderBy(p => p.Nombre)
        .Select(p => new ProyectoMinimoDto { Id = p.Id, Nombre = p.Nombre })
        .ToList();

    return Results.Ok(new MiTableroResponse
    {
        UsuarioNombreCompleto = usuarioActual.NombreCompleto,
        UsuarioId = usuarioActual.Id,
        EsOperario = esOperario,
        Proyectos = proyectos,
        Tareas = tareas.Select(MapTareaPersonal).ToList()
    });
})
.WithName("GetAdministracionMiTablero");

app.MapPost("/api/administracion/mi-tablero/tareas/{id:int}/estado", async (
    int id,
    CambiarEstadoTareaPersonalRequest request,
    TareaWorkflowService tareaWorkflowService) =>
{
    try
    {
        await tareaWorkflowService.ActualizarEstadoAsync(id, request.Estado);
        return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = "Estado de la tarea actualizado." });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = ex.Message });
    }
})
.WithName("PostAdministracionMiTableroEstado");

app.MapPost("/api/administracion/mi-tablero/tareas/{id:int}/bloquear", async (
    int id,
    BloquearTareaPersonalRequest request,
    TareaWorkflowService tareaWorkflowService) =>
{
    if (string.IsNullOrWhiteSpace(request.JustificacionTecnica))
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = "Debe proporcionar una justificacion tecnica." });
    }

    try
    {
        await tareaWorkflowService.BloquearAsync(id, request.Tipo, request.JustificacionTecnica.Trim());
        return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = "Tarea bloqueada correctamente." });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = ex.Message });
    }
})
.WithName("PostAdministracionMiTableroBloquear");

app.MapPost("/api/administracion/mi-tablero/tareas/{id:int}/desbloquear", async (
    int id,
    TareaWorkflowService tareaWorkflowService) =>
{
    try
    {
        await tareaWorkflowService.DesbloquearAsync(id);
        return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = "Tarea desbloqueada correctamente." });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = ex.Message });
    }
})
.WithName("PostAdministracionMiTableroDesbloquear");

app.MapPost("/api/administracion/mi-tablero/tareas/{id:int}/finalizar", async (
    int id,
    HttpContext httpContext,
    TareaWorkflowService tareaWorkflowService) =>
{
    var usuarioId = ObtenerUsuarioAutenticadoId(httpContext.User);
    if (string.IsNullOrWhiteSpace(usuarioId))
    {
        return Results.Unauthorized();
    }

    try
    {
        await tareaWorkflowService.CompletarAsync(id, usuarioId, "Finalizada desde tablero personal");
        return Results.Ok(new OperacionResponse { Correcto = true, Mensaje = "Tarea marcada como terminada." });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new OperacionResponse { Correcto = false, Mensaje = ex.Message });
    }
})
.WithName("PostAdministracionMiTableroFinalizar");

app.Run();

static bool UsuarioPuedeAccederRuta(ClaimsPrincipal user, PathString path)
{
    if (path.StartsWithSegments("/api/rrhh"))
    {
        return user.IsInRole("RecursosHumanos") || user.IsInRole("Administrador");
    }

    if (path.StartsWithSegments("/api/jefe-obra"))
    {
        return user.IsInRole("JefeObra") || user.IsInRole("OficinaTecnica") || user.IsInRole("Administrador");
    }

    if (path.StartsWithSegments("/api/operario"))
    {
        return user.IsInRole("Operario") ||
               user.IsInRole("OperarioObra") ||
               user.IsInRole("OperarioOficinaT") ||
               user.IsInRole("JefeObra") ||
               user.IsInRole("OficinaTecnica") ||
               user.IsInRole("RecursosHumanos") ||
               user.IsInRole("Administrador");
    }

    if (path.StartsWithSegments("/api/consultas/admin"))
    {
        return user.IsInRole("Administrador");
    }

    if (path.StartsWithSegments("/api/consultas/jefe-obra"))
    {
        return user.IsInRole("JefeObra") || user.IsInRole("Administrador");
    }

    if (path.StartsWithSegments("/api/consultas/oficina-tecnica"))
    {
        return user.IsInRole("OficinaTecnica") || user.IsInRole("Administrador");
    }

    if (path.StartsWithSegments("/api/administracion/usuarios"))
    {
        return user.IsInRole("Administrador");
    }

    if (path.StartsWithSegments("/api/administracion/tablero-proyectos"))
    {
        return user.IsInRole("Administrador");
    }

    if (path.StartsWithSegments("/api/administracion/empleados"))
    {
        return user.IsInRole("Administrador") || user.IsInRole("JefeObra") || user.IsInRole("OficinaTecnica");
    }

    if (path.StartsWithSegments("/api/administracion/mi-tablero"))
    {
        return true;
    }

    if (path.StartsWithSegments("/api/materiales/solicitudes/admin"))
    {
        return user.IsInRole("Administrador");
    }

    if (path.StartsWithSegments("/api/materiales/solicitudes/jefe-obra"))
    {
        return user.IsInRole("JefeObra") || user.IsInRole("Administrador");
    }

    if (path.StartsWithSegments("/api/materiales/proveedores") ||
        path.StartsWithSegments("/api/materiales/categorias"))
    {
        return user.IsInRole("Administrador");
    }

    if (path.StartsWithSegments("/api/proyectos") ||
        path.StartsWithSegments("/api/materiales") ||
        path.StartsWithSegments("/api/consultas"))
    {
        return true;
    }

    return true;
}

static ProveedorResumenDto MapProveedor(Proveedor proveedor) =>
    new(
        proveedor.Id,
        proveedor.Nombre,
        proveedor.CIF,
        proveedor.Direccion,
        proveedor.Telefono,
        proveedor.Email,
        proveedor.Activo);

static CategoriaMaterialResumenDto MapCategoria(CategoriaMaterial categoria) =>
    new(categoria.Id, categoria.Nombre, categoria.Descripcion, categoria.Activa);

static MaterialResumenDto MapMaterial(Material material) =>
    new(
        material.Id,
        material.Codigo,
        material.Nombre,
        material.Descripcion,
        material.Activo,
        material.PrecioUnitario,
        material.UnidadMedida,
        material.StockDisponible,
        material.StockMinimo,
        material.Categoria,
        material.ProveedorId,
        material.Proveedor != null ? MapProveedor(material.Proveedor) : null,
        material.Proveedores.Select(MapProveedor).ToList());

static ProyectoConsultaDto MapProyectoConsulta(Proyecto proyecto) =>
    new(
        proyecto.Id,
        proyecto.Nombre,
        proyecto.Municipio,
        proyecto.Provincia,
        proyecto.Estado,
        proyecto.FechaInicio,
        proyecto.Tareas?.Count ?? 0,
        proyecto.Presupuesto != null);

static TareaPersonalDto MapTareaPersonal(Tarea tarea) =>
    new()
    {
        Id = tarea.Id,
        Nombre = tarea.Nombre,
        Descripcion = tarea.Descripcion,
        Estado = tarea.Estado,
        FechaInicio = tarea.FechaInicio,
        FechaFin = tarea.FechaFin,
        PresupuestoEstimado = tarea.PresupuestoEstimado,
        CostesReales = tarea.CostesReales,
        ProyectoId = tarea.ProyectoId,
        Proyecto = new ProyectoMinimoDto { Id = tarea.Proyecto.Id, Nombre = tarea.Proyecto.Nombre },
        TareaPadreId = tarea.TareaPadreId,
        TareaPadreNombre = tarea.TareaPadre?.Nombre,
        Nivel = tarea.Nivel,
        Prioridad = tarea.Prioridad,
        UsuariosAsignadosCount = tarea.UsuariosAsignados.Count,
        Bloqueo = tarea.Bloqueo == null ? null : new BloqueoResumenDto
        {
            Id = tarea.Bloqueo.Id,
            Tipo = tarea.Bloqueo.Tipo,
            JustificacionTecnica = tarea.Bloqueo.JustificacionTecnica,
            FechaBloqueo = tarea.Bloqueo.FechaBloqueo,
            FechaResolucion = tarea.Bloqueo.FechaResolucion
        },
        CompletadaPorId = tarea.CompletadaPorId,
        FechaFinalizacion = tarea.FechaFinalizacion,
        ObservacionesFinalizacion = tarea.ObservacionesFinalizacion
    };

static bool PuedeVerUsuarioPorJerarquia(HashSet<string> rolesUsuarioActual, IList<string> rolesEmpleado)
{
    if (rolesUsuarioActual.Contains("Administrador"))
    {
        return true;
    }

    if (rolesUsuarioActual.Contains("JefeObra"))
    {
        return rolesEmpleado.Contains("OperarioObra") || rolesEmpleado.Contains("Operario");
    }

    if (rolesUsuarioActual.Contains("OficinaTecnica"))
    {
        return rolesEmpleado.Contains("OperarioOficinaT") || rolesEmpleado.Contains("Operario");
    }

    return false;
}

static string ObtenerDescripcionJerarquia(HashSet<string> rolesUsuarioActual)
{
    if (rolesUsuarioActual.Contains("Administrador"))
    {
        return "Como Administrador ves todos los escalones.";
    }

    if (rolesUsuarioActual.Contains("JefeObra"))
    {
        return "Como Jefe de Obra ves y gestionas Operario-Obra.";
    }

    if (rolesUsuarioActual.Contains("OficinaTecnica"))
    {
        return "Como Oficina Tecnica ves y gestionas Operario-OficinaT.";
    }

    return "Tu rol no tiene un escalon inferior configurable en este modulo.";
}

static (string nombre, string apellidos) SepararNombreCompleto(string? nombreCompleto)
{
    if (string.IsNullOrWhiteSpace(nombreCompleto))
    {
        return ("Sin", "Nombre");
    }

    var partes = nombreCompleto.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
    if (partes.Length == 1)
    {
        return (partes[0], string.Empty);
    }

    return (partes[0], string.Join(' ', partes.Skip(1)));
}

static SolicitudMaterialResumenDto MapSolicitud(SolicitudMaterial solicitud) =>
    new(
        solicitud.Id,
        solicitud.MaterialId,
        MapMaterial(solicitud.Material),
        solicitud.ProyectoId,
        new ProyectoResumenDto(solicitud.Proyecto.Id, solicitud.Proyecto.Nombre),
        solicitud.CantidadSolicitada,
        solicitud.Justificacion,
        solicitud.SolicitadoPorId,
        new UsuarioResumenDto(solicitud.SolicitadoPor.Id, solicitud.SolicitadoPor.NombreCompleto, solicitud.SolicitadoPor.Cargo),
        solicitud.FechaSolicitud,
        solicitud.Estado,
        solicitud.RevisadoPorId,
        solicitud.RevisadoPor != null
            ? new UsuarioResumenDto(solicitud.RevisadoPor.Id, solicitud.RevisadoPor.NombreCompleto, solicitud.RevisadoPor.Cargo)
            : null,
        solicitud.FechaRespuesta,
        solicitud.ObservacionesAdmin,
        solicitud.Prioridad,
        solicitud.FechaNecesaria);

static string? ObtenerUsuarioAutenticadoId(ClaimsPrincipal user)
{
    return user.FindFirstValue(ClaimTypes.NameIdentifier);
}
