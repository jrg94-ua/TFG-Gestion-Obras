using GestionObras.Web.Components;
using GestionObras.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GestionObras.Core.Entities;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

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
    options.AddPolicy("OperarioPolicy", policy => policy.RequireRole("Operario", "JefeObra", "Administrador"));
});

// Agregar servicios de cascading authentication state para Blazor
builder.Services.AddCascadingAuthenticationState();

// Agregar HttpContextAccessor para acceder al contexto HTTP en componentes
builder.Services.AddHttpContextAccessor();

// Registrar repositorios
builder.Services.AddScoped<GestionObras.Infrastructure.Repositories.IProyectoRepository, GestionObras.Infrastructure.Repositories.ProyectoRepository>();
builder.Services.AddScoped<GestionObras.Infrastructure.Repositories.ITareaRepository, GestionObras.Infrastructure.Repositories.TareaRepository>();
builder.Services.AddScoped<GestionObras.Infrastructure.Repositories.IEmpleadoRepository, GestionObras.Infrastructure.Repositories.EmpleadoRepository>();

// Registrar HttpClient para CatastroService
builder.Services.AddHttpClient<GestionObras.Web.Services.ICatastroService, GestionObras.Web.Services.CatastroService>();

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
        // Aplicar migraciones automáticamente
        var dbContext = services.GetRequiredService<GestionObrasDbContext>();
        
        // Asegurar que la base de datos se cree si no existe
        await dbContext.Database.EnsureCreatedAsync();
        
        var userManager = services.GetRequiredService<UserManager<UsuarioObra>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await InicializarRolesYUsuarios(roleManager, userManager);
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
    string[] roles = { "Administrador", "JefeObra", "OficinaTecnica", "Operario" };
    
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
