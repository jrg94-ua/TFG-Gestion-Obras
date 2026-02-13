using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GestionObras.Core.Entities;

namespace GestionObras.Infrastructure.Data;

/// <summary>
/// Contexto de base de datos principal del sistema de gestión de obras
/// Extiende IdentityDbContext para soporte completo de autenticación y autorización
/// </summary>
public class GestionObrasDbContext : IdentityDbContext<UsuarioObra>
{
    public GestionObrasDbContext(DbContextOptions<GestionObrasDbContext> options) : base(options)
    {
    }

    // DbSets principales - RF-01 a RF-04
    public DbSet<Proyecto> Proyectos { get; set; }
    public DbSet<Tarea> Tareas { get; set; }
    public DbSet<BloqueoTarea> BloqueosTareas { get; set; }
    public DbSet<DocumentoTarea> DocumentosTareas { get; set; }
    public DbSet<FirmaTarea> FirmasTareas { get; set; }
    public DbSet<Empleado> Empleados { get; set; }
    public DbSet<Material> Materiales { get; set; }
    public DbSet<SolicitudMaterial> SolicitudesMateriales { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<Presupuesto> Presupuestos { get; set; }
    public DbSet<CarpetaLegal> CarpetasLegales { get; set; }
    public DbSet<Proveedor> Proveedores { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuraciones básicas de las entidades
        ConfigurarIdentity(builder);
        ConfigurarRelaciones(builder);
    }

    private void ConfigurarIdentity(ModelBuilder builder)
    {
        // Configuraciones de Identity personalizadas
        builder.Entity<UsuarioObra>(entity =>
        {
            entity.Property(u => u.DNI).HasMaxLength(20);
            entity.Property(u => u.NombreCompleto).HasMaxLength(200);
            entity.Property(u => u.TelefonoMovil).HasMaxLength(20);
            entity.Property(u => u.Cargo).HasMaxLength(100);
            
            // Índices para búsquedas rápidas
            entity.HasIndex(u => u.DNI).IsUnique().HasFilter("[DNI] IS NOT NULL");
        });
    }

    private void ConfigurarRelaciones(ModelBuilder builder)
    {
        // Proyecto
        builder.Entity<Proyecto>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Provincia).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Municipio).HasMaxLength(100).IsRequired();
            
            // Relación con Responsable
            entity.HasOne(p => p.Responsable)
                  .WithMany()
                  .HasForeignKey(p => p.ResponsableId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(p => p.CarpetaLegal)
                  .WithOne(c => c.Proyecto)
                  .HasForeignKey<CarpetaLegal>(c => c.ProyectoId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(p => p.Presupuesto)
                  .WithOne(pr => pr.Proyecto)
                  .HasForeignKey<Presupuesto>(pr => pr.ProyectoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Tarea
        builder.Entity<Tarea>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Descripcion).HasMaxLength(1000);
            entity.Property(t => t.PresupuestoEstimado).HasColumnType("decimal(18,2)");
            entity.Property(t => t.CostesReales).HasColumnType("decimal(18,2)");
            
            entity.HasOne(t => t.Proyecto)
                  .WithMany(p => p.Tareas)
                  .HasForeignKey(t => t.ProyectoId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            // Configurar relación many-to-many con UsuarioObra
            entity.HasMany(t => t.UsuariosAsignados)
                  .WithMany()
                  .UsingEntity<Dictionary<string, object>>(
                      "TareaUsuarioObra",
                      j => j.HasOne<UsuarioObra>()
                            .WithMany()
                            .HasForeignKey("UsuariosAsignadosId")
                            .OnDelete(DeleteBehavior.Cascade),
                      j => j.HasOne<Tarea>()
                            .WithMany()
                            .HasForeignKey("TareasId")
                            .OnDelete(DeleteBehavior.Cascade)
                  );
                  
            // Configurar jerarquía de tareas (auto-referencia)
            entity.HasOne(t => t.TareaPadre)
                  .WithMany(t => t.SubTareas)
                  .HasForeignKey(t => t.TareaPadreId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Dependencias entre tareas (predecesoras/dependientes)
            entity.HasMany(t => t.Predecesoras)
                  .WithMany(t => t.Dependientes)
                  .UsingEntity<Dictionary<string, object>>(
                      "TareaDependencias",
                      j => j.HasOne<Tarea>()
                            .WithMany()
                            .HasForeignKey("PredecesoraId")
                            .OnDelete(DeleteBehavior.Restrict),
                      j => j.HasOne<Tarea>()
                            .WithMany()
                            .HasForeignKey("TareaId")
                            .OnDelete(DeleteBehavior.Cascade),
                      j =>
                      {
                          j.HasKey("TareaId", "PredecesoraId");
                          j.ToTable("TareaDependencias");
                      }
                  );
                  
            // Relación con usuario que completó la tarea
            entity.HasOne(t => t.CompletadaPor)
                  .WithMany()
                  .HasForeignKey(t => t.CompletadaPorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
        
        // DocumentoTarea
        builder.Entity<DocumentoTarea>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.NombreArchivo).HasMaxLength(255).IsRequired();
            entity.Property(d => d.RutaArchivo).HasMaxLength(500).IsRequired();
            entity.Property(d => d.TipoMime).HasMaxLength(100);
            entity.Property(d => d.Descripcion).HasMaxLength(1000);
            
            entity.HasOne(d => d.Tarea)
                  .WithMany(t => t.Documentos)
                  .HasForeignKey(d => d.TareaId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(d => d.UsuarioSubida)
                  .WithMany()
                  .HasForeignKey(d => d.UsuarioSubidaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // FirmaTarea
        builder.Entity<FirmaTarea>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Observaciones).HasMaxLength(1000);
            entity.Property(f => f.MotivoRechazo).HasMaxLength(500);
            
            entity.HasOne(f => f.Tarea)
                  .WithMany(t => t.Firmas)
                  .HasForeignKey(f => f.TareaId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(f => f.Usuario)
                  .WithMany()
                  .HasForeignKey(f => f.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            // Un usuario solo puede firmar una vez cada tarea
            entity.HasIndex(f => new { f.TareaId, f.UsuarioId })
                  .IsUnique();
        });

        // Presupuesto
        builder.Entity<Presupuesto>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Total).HasColumnType("decimal(18,2)");
        });

        // Empleado
        builder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Apellidos).HasMaxLength(200).IsRequired();
            entity.Property(e => e.DNI).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            
            entity.HasIndex(e => e.DNI).IsUnique();
        });

        // Material
        builder.Entity<Material>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(m => m.Codigo).HasMaxLength(50);
            entity.Property(m => m.Descripcion).HasMaxLength(1000);
            entity.Property(m => m.PrecioUnitario).HasColumnType("decimal(18,2)");
            
            // Propiedades técnicas CTE
            entity.Property(m => m.TransmitanciaTermica).HasColumnType("decimal(18,4)");
            entity.Property(m => m.Densidad).HasColumnType("decimal(18,4)");
            entity.Property(m => m.ResistenciaCompresion).HasColumnType("decimal(18,4)");
        });

        // Factura
        builder.Entity<Factura>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Importe).HasColumnType("decimal(18,2)");
            entity.Property(f => f.Concepto).HasMaxLength(500);
            
            entity.HasOne(f => f.Proveedor)
                  .WithMany()
                  .HasForeignKey(f => f.ProveedorId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(f => f.Proyecto)
                  .WithMany()
                  .HasForeignKey(f => f.ProyectoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Proveedor
        builder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(p => p.CIF).HasMaxLength(20).IsRequired();
            entity.Property(p => p.Email).HasMaxLength(200);
            entity.Property(p => p.Telefono).HasMaxLength(20);
            entity.Property(p => p.Direccion).HasMaxLength(500);
            
            entity.HasIndex(p => p.CIF).IsUnique();
        });

        // Carpeta Legal
        builder.Entity<CarpetaLegal>(entity =>
        {
            entity.HasKey(c => c.Id);
        });

        // SolicitudMaterial
        builder.Entity<SolicitudMaterial>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.CantidadSolicitada).HasColumnType("decimal(18,2)");
            entity.Property(s => s.Justificacion).HasMaxLength(1000).IsRequired();
            entity.Property(s => s.ObservacionesAdmin).HasMaxLength(1000);
            
            // Relación con Material
            entity.HasOne(s => s.Material)
                  .WithMany()
                  .HasForeignKey(s => s.MaterialId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Relación con Proyecto
            entity.HasOne(s => s.Proyecto)
                  .WithMany()
                  .HasForeignKey(s => s.ProyectoId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Relación con Usuario solicitante (Jefe de Obra)
            entity.HasOne(s => s.SolicitadoPor)
                  .WithMany()
                  .HasForeignKey(s => s.SolicitadoPorId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Relación con Usuario que revisa (Admin)
            entity.HasOne(s => s.RevisadoPor)
                  .WithMany()
                  .HasForeignKey(s => s.RevisadoPorId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Índices para búsquedas
            entity.HasIndex(s => s.Estado);
            entity.HasIndex(s => s.FechaSolicitud);
        });
    }
}
