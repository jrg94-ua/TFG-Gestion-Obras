using System.Data;
using System.Reflection;
using GestionObras.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GestionObras.Infrastructure.Services;

public sealed class DatabaseMigrationService
{
    private const string HistoryTableName = "__EFMigrationsHistory";
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public DatabaseMigrationService(
        GestionObrasDbContext db,
        IConfiguration configuration,
        ILogger<DatabaseMigrationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task ApplyAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseExistsAsync(cancellationToken);

        await using var db = CreateMigrationDbContext();
        EnsureConnectionStringInitialized(db);

        if (!await HasAnyUserTableAsync(db, cancellationToken))
        {
            var createScript = db.Database.GenerateCreateScript();
            await ExecuteSqlScriptAsync(GetRequiredConnectionString(), createScript, cancellationToken);
            return;
        }

        if (await IsLegacyDatabaseWithoutMigrationHistoryAsync(db, cancellationToken))
        {
            await BaselineLegacyDatabaseAsync(db, cancellationToken);
            return;
        }

        _logger.LogInformation("La base de datos ya contiene esquema. Se omite migracion en caliente durante el arranque.");
    }

    private async Task<bool> IsLegacyDatabaseWithoutMigrationHistoryAsync(GestionObrasDbContext db, CancellationToken cancellationToken)
    {
        if (await TableExistsAsync(db, HistoryTableName, cancellationToken))
        {
            return false;
        }

        return await HasAnyUserTableAsync(db, cancellationToken);
    }

    private async Task BaselineLegacyDatabaseAsync(GestionObrasDbContext db, CancellationToken cancellationToken)
    {
        var migrationsAssembly = db.GetService<IMigrationsAssembly>();
        var migrationIds = migrationsAssembly.Migrations.Keys.ToList();

        if (migrationIds.Count == 0)
        {
            _logger.LogWarning("Se ha detectado una base de datos heredada sin historial de migraciones, pero no existen migraciones locales para registrarla.");
            return;
        }

        _logger.LogWarning("Base de datos heredada detectada sin historial de migraciones. Se registrara la migracion actual como baseline.");

        var historyRepository = db.GetService<IHistoryRepository>();
        await ExecuteSqlScriptAsync(GetRequiredConnectionString(), historyRepository.GetCreateScript(), cancellationToken);

        foreach (var migrationId in migrationIds)
        {
            var sql = historyRepository.GetInsertScript(new HistoryRow(migrationId, GetEfProductVersion()));
            await ExecuteSqlScriptAsync(GetRequiredConnectionString(), sql, cancellationToken);
        }
    }

    private async Task<bool> TableExistsAsync(GestionObrasDbContext db, string tableName, CancellationToken cancellationToken)
    {
        EnsureConnectionStringInitialized(db);

        await using var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT CASE WHEN OBJECT_ID(N'[dbo].[{tableName}]', N'U') IS NULL THEN 0 ELSE 1 END";
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result) == 1;
    }

    private async Task<bool> HasAnyUserTableAsync(GestionObrasDbContext db, CancellationToken cancellationToken)
    {
        EnsureConnectionStringInitialized(db);

        await using var connection = db.Database.GetDbConnection();
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

    private async Task EnsureDatabaseExistsAsync(CancellationToken cancellationToken)
    {
        var configuredConnectionString = GetRequiredConnectionString();

        var targetBuilder = new SqlConnectionStringBuilder(configuredConnectionString);
        var databaseName = targetBuilder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("La cadena de conexion 'DefaultConnection' no define una base de datos destino.");
        }

        var masterBuilder = new SqlConnectionStringBuilder(configuredConnectionString)
        {
            InitialCatalog = "master"
        };

        await using var connection = new SqlConnection(masterBuilder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $"IF DB_ID(N'{databaseName.Replace("'", "''")}') IS NULL CREATE DATABASE [{databaseName.Replace("]", "]]")}]";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private void EnsureConnectionStringInitialized(GestionObrasDbContext db)
    {
        var connection = db.Database.GetDbConnection();
        if (!string.IsNullOrWhiteSpace(connection.ConnectionString))
        {
            return;
        }

        var configuredConnectionString = GetRequiredConnectionString();

        db.Database.SetConnectionString(configuredConnectionString);
        connection.ConnectionString = configuredConnectionString;
        _logger.LogInformation("Cadena de conexion aplicada explicitamente en DatabaseMigrationService.");
    }

    private string GetRequiredConnectionString()
    {
        return _configuration.GetConnectionString("DefaultConnection") ??
               _configuration["ConnectionStrings:DefaultConnection"] ??
               throw new InvalidOperationException("No se ha encontrado la cadena de conexion 'DefaultConnection'.");
    }

    private GestionObrasDbContext CreateMigrationDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<GestionObrasDbContext>();
        optionsBuilder.UseSqlServer(GetRequiredConnectionString(), sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null));

        return new GestionObrasDbContext(optionsBuilder.Options);
    }

    private static async Task ExecuteSqlScriptAsync(string connectionString, string script, CancellationToken cancellationToken)
    {
        var batches = SplitSqlBatches(script);
        if (batches.Count == 0)
        {
            return;
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (var batch in batches)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = batch;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static List<string> SplitSqlBatches(string script)
    {
        var batches = new List<string>();
        var current = new List<string>();

        using var reader = new StringReader(script);
        while (reader.ReadLine() is { } line)
        {
            if (string.Equals(line.Trim(), "GO", StringComparison.OrdinalIgnoreCase))
            {
                AddBatchIfAny(batches, current);
                current.Clear();
                continue;
            }

            current.Add(line);
        }

        AddBatchIfAny(batches, current);
        return batches;
    }

    private static void AddBatchIfAny(List<string> batches, List<string> lines)
    {
        var sql = string.Join(Environment.NewLine, lines).Trim();
        if (!string.IsNullOrWhiteSpace(sql))
        {
            batches.Add(sql);
        }
    }
}
