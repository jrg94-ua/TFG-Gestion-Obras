using GestionObras.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionObras.UnitTests;

internal static class TestDbContextFactory
{
    public static GestionObrasDbContext Create()
    {
        var options = new DbContextOptionsBuilder<GestionObrasDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new GestionObrasDbContext(options);
    }
}
