using System.Data;
using System.Reflection;
using GestionObras.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;

namespace GestionObras.Infrastructure.Services;

public sealed class DatabaseMigrationService
{
    private const string HistoryTableName = "__EFMigrationsHistory";
    private readonly GestionObrasDbContext _db;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public DatabaseMigrationService(GestionObrasDbContext db, ILogger<DatabaseMigrationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        if (await IsLegacyDatabaseWithoutMigrationHistoryAsync(cancellationToken))
        {
            await BaselineLegacyDatabaseAsync(cancellationToken);
            return;
        }

        await _db.Database.MigrateAsync(cancellationToken);
    }

    private async Task<bool> IsLegacyDatabaseWithoutMigrationHistoryAsync(CancellationToken cancellationToken)
    {
        if (await TableExistsAsync(HistoryTableName, cancellationToken))
        {
            return false;
        }

        return await HasAnyUserTableAsync(cancellationToken);
    }

    private async Task BaselineLegacyDatabaseAsync(CancellationToken cancellationToken)
    {
        var migrationsAssembly = _db.GetService<IMigrationsAssembly>();
        var migrationIds = migrationsAssembly.Migrations.Keys.ToList();

        if (migrationIds.Count == 0)
        {
            _logger.LogWarning("Se ha detectado una base de datos heredada sin historial de migraciones, pero no existen migraciones locales para registrarla.");
            return;
        }

        _logger.LogWarning("Base de datos heredada detectada sin historial de migraciones. Se registrara la migracion actual como baseline.");

        var historyRepository = _db.GetService<IHistoryRepository>();
        await _db.Database.ExecuteSqlRawAsync(historyRepository.GetCreateScript(), cancellationToken);

        foreach (var migrationId in migrationIds)
        {
            var sql = historyRepository.GetInsertScript(new HistoryRow(migrationId, GetEfProductVersion()));
            await _db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }
    }

    private async Task<bool> TableExistsAsync(string tableName, CancellationToken cancellationToken)
    {
        await using var connection = _db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT CASE WHEN OBJECT_ID(N'[dbo].[{tableName}]', N'U') IS NULL THEN 0 ELSE 1 END";
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result) == 1;
    }

    private async Task<bool> HasAnyUserTableAsync(CancellationToken cancellationToken)
    {
        await using var connection = _db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT CASE
                WHEN EXISTS (
                    SELECT 1
                    FROM sys.tables
                    WHERE is_ms_shipped = 0
                      AND name <> '__EFMigrationsHistory'
                ) THEN 1
                ELSE 0
            END
            """;
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result) == 1;
    }

    private static string GetEfProductVersion()
    {
        return typeof(DbContext).Assembly
                   .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                   .InformationalVersion?
                   .Split('+')[0]
               ?? "10.0.0";
    }
}
