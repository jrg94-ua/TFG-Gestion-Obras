using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GestionObras.Infrastructure.Data;

public sealed class GestionObrasDbContextFactory : IDesignTimeDbContextFactory<GestionObrasDbContext>
{
    public GestionObrasDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ??
            Environment.GetEnvironmentVariable("ConnectionStrings:DefaultConnection") ??
            "Server=localhost,14330;Database=GestionObrasDB;User Id=sa;Password=GestionObras2026!;TrustServerCertificate=True;MultipleActiveResultSets=true";

        var optionsBuilder = new DbContextOptionsBuilder<GestionObrasDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new GestionObrasDbContext(optionsBuilder.Options);
    }
}
