using GestionObras.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GestionObras.IntegrationTests;

internal sealed class SqliteTestDbContextFactory : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteTestDbContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public GestionObrasDbContext Create()
    {
        var options = new DbContextOptionsBuilder<GestionObrasDbContext>()
            .UseSqlite(_connection)
            .Options;

        var db = new GestionObrasDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    public ValueTask DisposeAsync()
    {
        _connection.Dispose();
        return ValueTask.CompletedTask;
    }
}
