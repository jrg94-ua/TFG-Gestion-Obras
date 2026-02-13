using GestionObras.Web.Components;
using GestionObras.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GestionObras.Core.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using System.Globalization;
using GestionObras.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar cultura española para formateo de moneda y fechas
var cultureInfo = new CultureInfo("es-ES");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Configurar DbContext con SQL Server
builder.Services.AddDbContext<GestionObrasDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        )
    ));

// Configurar ASP.NET Core Identity
builder.Services.AddIdentity<UsuarioObra, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<GestionObrasDbContext>()
.AddDefaultTokenProviders();

// Configurar autenticación y autorización
builder.Services.AddAuthentication();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("JefeObraPolicy", policy => policy.RequireRole("JefeObra", "Administrador"));
    options.AddPolicy("OficinaTecnicaPolicy", policy => policy.RequireRole("OficinaTecnica", "Administrador"));
    options.AddPolicy("OperarioPolicy", policy => policy.RequireRole("Operario", "OperarioObra", "OperarioOficinaT", "JefeObra", "OficinaTecnica", "Administrador"));
});

// Agregar servicios de cascading authentication state para Blazor
builder.Services.AddCascadingAuthenticationState();

// Agregar HttpContextAccessor para acceder al contexto HTTP en componentes
builder.Services.AddHttpContextAccessor();

// Registrar repositorios
builder.Services.AddScoped<GestionObras.Infrastructure.Repositories.IProyectoRepository, GestionObras.Infrastructure.Repositories.ProyectoRepository>();
builder.Services.AddScoped<GestionObras.Infrastructure.Repositories.ITareaRepository, GestionObras.Infrastructure.Repositories.TareaRepository>();
builder.Services.AddScoped<GestionObras.Infrastructure.Repositories.IEmpleadoRepository, GestionObras.Infrastructure.Repositories.EmpleadoRepository>();

// Registrar servicios personalizados
builder.Services.AddScoped<GestionObras.Web.Services.DocumentoService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configurar opciones de cookies para autenticación
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
});

var app = builder.Build();

// Inicializar roles y usuarios por defecto
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Crear la base de datos si no existe
        var dbContext = services.GetRequiredService<GestionObrasDbContext>();
        
        // Crear la base de datos con el esquema actual
        await dbContext.Database.EnsureCreatedAsync();
        await AsegurarEsquemaDependenciasAsync(dbContext);
        
        var userManager = services.GetRequiredService<UserManager<UsuarioObra>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var startupLogger = services.GetRequiredService<ILogger<Program>>();
        await InicializarRolesYUsuarios(roleManager, userManager);

        var seedOnStartup = builder.Configuration.GetValue<bool?>("SeedDemoOnStartup")
            ?? app.Environment.IsDevelopment();

        if (seedOnStartup)
        {
            startupLogger.LogInformation("Seed demo activado en arranque de contenedor.");
            await DemoDataSeeder.SeedAsync(dbContext, userManager, roleManager);
        }
        else
        {
            startupLogger.LogInformation("Seed demo desactivado por configuración (SeedDemoOnStartup=false).");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al inicializar roles y usuarios");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

/// <summary>
/// Inicializa los roles y usuarios por defecto del sistema
/// </summary>
static async Task InicializarRolesYUsuarios(RoleManager<IdentityRole> roleManager, UserManager<UsuarioObra> userManager)
{
    // Crear roles si no existen
    string[] roles = { "Administrador", "JefeObra", "OficinaTecnica", "Operario", "OperarioObra", "OperarioOficinaT" };
    
    foreach (var rol in roles)
    {
        if (!await roleManager.RoleExistsAsync(rol))
        {
            await roleManager.CreateAsync(new IdentityRole(rol));
        }
    }

    // Crear usuario administrador por defecto si no existe
    var adminEmail = "admin@gestionobras.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        adminUser = new UsuarioObra
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            NombreCompleto = "Administrador del Sistema",
            DNI = "00000000A",
            TipoUsuario = TipoUsuario.Administrador,
            Activo = true,
            FechaCreacion = DateTime.Now
        };
        
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Administrador");
        }
    }

    // Crear usuario jefe de obra de prueba
    var jefeEmail = "jefe@gestionobras.com";
    var jefeUser = await userManager.FindByEmailAsync(jefeEmail);
    
    if (jefeUser == null)
    {
        jefeUser = new UsuarioObra
        {
            UserName = jefeEmail,
            Email = jefeEmail,
            EmailConfirmed = true,
            NombreCompleto = "Jefe de Obra Demo",
            DNI = "11111111B",
            TipoUsuario = TipoUsuario.JefeObra,
            Activo = true,
            FechaCreacion = DateTime.Now
        };
        
        var result = await userManager.CreateAsync(jefeUser, "Jefe123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(jefeUser, "JefeObra");
        }
    }
}

static async Task AsegurarEsquemaDependenciasAsync(GestionObrasDbContext dbContext)
{
    const string sql = @"
IF OBJECT_ID(N'[dbo].[TareaDependencias]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[TareaDependencias]
    (
        [TareaId] INT NOT NULL,
        [PredecesoraId] INT NOT NULL,
        CONSTRAINT [PK_TareaDependencias] PRIMARY KEY ([TareaId], [PredecesoraId]),
        CONSTRAINT [FK_TareaDependencias_Tareas_TareaId]
            FOREIGN KEY ([TareaId]) REFERENCES [dbo].[Tareas]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_TareaDependencias_Tareas_PredecesoraId]
            FOREIGN KEY ([PredecesoraId]) REFERENCES [dbo].[Tareas]([Id])
    );

    CREATE INDEX [IX_TareaDependencias_PredecesoraId]
        ON [dbo].[TareaDependencias]([PredecesoraId]);
END";

    await dbContext.Database.ExecuteSqlRawAsync(sql);
}
