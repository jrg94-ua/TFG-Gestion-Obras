using GestionObras.Core.Entities;
using GestionObras.Infrastructure.Data;
using GestionObras.Infrastructure.Services;
using GestionObras.Web.Components;
using GestionObras.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
var dataProtectionPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "shared-keys"));
Directory.CreateDirectory(dataProtectionPath);

// Configurar cultura espanola para formateo de moneda y fechas
var cultureInfo = new CultureInfo("es-ES");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
    .SetApplicationName("GestionObras.Auth");

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

// Configurar autenticacion y autorizacion
builder.Services.AddAuthentication();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("JefeObraPolicy", policy => policy.RequireRole("JefeObra", "Administrador"));
    options.AddPolicy("OficinaTecnicaPolicy", policy => policy.RequireRole("OficinaTecnica", "Administrador"));
    options.AddPolicy("RecursosHumanosPolicy", policy => policy.RequireRole("RecursosHumanos", "Administrador"));
    options.AddPolicy("OperarioPolicy", policy => policy.RequireRole("Operario", "OperarioObra", "OperarioOficinaT", "JefeObra", "OficinaTecnica", "RecursosHumanos", "Administrador"));
});

// Agregar servicios de cascading authentication state para Blazor
builder.Services.AddCascadingAuthenticationState();

// Agregar HttpContextAccessor para acceder al contexto HTTP en componentes
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ApiAuthCookieHandler>();
builder.Services.AddHttpClient<JefeObraApiClient>((sp, client) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<ApiAuthCookieHandler>();
builder.Services.AddHttpClient<OperarioApiClient>((sp, client) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<ApiAuthCookieHandler>();
builder.Services.AddHttpClient<RRHHApiClient>((sp, client) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<ApiAuthCookieHandler>();
builder.Services.AddHttpClient<ProyectosApiClient>((sp, client) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<ApiAuthCookieHandler>();
builder.Services.AddHttpClient<MaterialesApiClient>((sp, client) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<ApiAuthCookieHandler>();
builder.Services.AddHttpClient<ConsultasApiClient>((sp, client) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<ApiAuthCookieHandler>();
builder.Services.AddHttpClient<AdministracionApiClient>((sp, client) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<ApiAuthCookieHandler>();

// Registrar repositorios
builder.Services.AddScoped<GestionObras.Infrastructure.Repositories.IProyectoRepository, GestionObras.Infrastructure.Repositories.ProyectoRepository>();
builder.Services.AddScoped<GestionObras.Infrastructure.Repositories.ITareaRepository, GestionObras.Infrastructure.Repositories.TareaRepository>();
builder.Services.AddScoped<GestionObras.Infrastructure.Repositories.IEmpleadoRepository, GestionObras.Infrastructure.Repositories.EmpleadoRepository>();
builder.Services.AddScoped<GestionObras.Infrastructure.Repositories.IFichajeRepository, GestionObras.Infrastructure.Repositories.FichajeRepository>();

// Registrar servicios personalizados
builder.Services.AddScoped<DocumentoService>();
builder.Services.AddScoped<ExportPdfService>();
builder.Services.AddScoped<ExportExcelService>();
builder.Services.AddScoped<FacturaService>();
builder.Services.AddScoped<DatabaseMigrationService>();
builder.Services.AddScoped<TareaWorkflowService>();
builder.Services.AddScoped<KanbanService>();
builder.Services.AddScoped<PresupuestoService>();
builder.Services.AddScoped<MaterialService>();
builder.Services.AddScoped<RRHHHorariosService>();
builder.Services.AddScoped<PlanificacionHorarioService>();
builder.Services.AddScoped<DashboardService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "GestionObras.Auth";
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
});

var app = builder.Build();

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<GestionObrasDbContext>();
        var migrationService = services.GetRequiredService<DatabaseMigrationService>();
        await migrationService.ApplyAsync();

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
            startupLogger.LogInformation("Seed demo desactivado por configuracion (SeedDemoOnStartup=false).");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al inicializar roles y usuarios");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
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

static async Task InicializarRolesYUsuarios(RoleManager<IdentityRole> roleManager, UserManager<UsuarioObra> userManager)
{
    string[] roles = { "Administrador", "JefeObra", "OficinaTecnica", "Operario", "OperarioObra", "OperarioOficinaT", "RecursosHumanos" };

    foreach (var rol in roles)
    {
        if (!await roleManager.RoleExistsAsync(rol))
        {
            await roleManager.CreateAsync(new IdentityRole(rol));
        }
    }

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
