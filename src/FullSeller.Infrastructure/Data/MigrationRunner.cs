using Npgsql;
using Microsoft.Extensions.Logging;

namespace FullSeller.Infrastructure.Data;

/// <summary>Простой раннер .sql-миграций из папки Data/Migrations, без сторонних зависимостей
/// (замена DbUp/FluentMigrator для минимального стека). Применяет файлы по имени (0001_, 0002_, ...),
/// отслеживая уже применённые в таблице __SchemaMigrations.</summary>
public class MigrationRunner
{
    private readonly ISqlConnectionFactory _factory;
    private readonly ILogger<MigrationRunner> _logger;
    private readonly string _migrationsPath;

    public MigrationRunner(ISqlConnectionFactory factory, ILogger<MigrationRunner> logger)
    {
        _factory = factory;
        _logger = logger;
        _migrationsPath = Path.Combine(AppContext.BaseDirectory, "Data", "Migrations");
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        await using var connection = await _factory.CreateOpenConnectionAsync(ct);

        await using (var ensureTable = connection.CreateCommand())
        {
            ensureTable.CommandText = """
                CREATE TABLE IF NOT EXISTS __SchemaMigrations (
                    FileName VARCHAR(260) NOT NULL PRIMARY KEY,
                    AppliedAt TIMESTAMPTZ NOT NULL DEFAULT now()
                );
                """;
            await ensureTable.ExecuteNonQueryAsync(ct);
        }

        if (!Directory.Exists(_migrationsPath))
        {
            _logger.LogWarning("Папка миграций {Path} не найдена — пропускаю.", _migrationsPath);
            return;
        }

        var files = Directory.GetFiles(_migrationsPath, "*.sql").OrderBy(f => f, StringComparer.Ordinal);
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);

            await using var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM __SchemaMigrations WHERE FileName = @FileName";
            checkCmd.Parameters.AddWithValue("@FileName", fileName);
            var already = (int)(long)(await checkCmd.ExecuteScalarAsync(ct) ?? 0L) > 0;
            if (already) continue;

            _logger.LogInformation("Применяю миграцию {FileName}...", fileName);
            var sql = await File.ReadAllTextAsync(file, ct);

            await using var transaction = (NpgsqlTransaction)await connection.BeginTransactionAsync(ct);
            try
            {
                // PostgreSQL (в отличие от T-SQL) умеет выполнять несколько ';'-разделённых операторов
                // одной командой — пакетный разделитель "GO" (специфика SSMS/sqlcmd) здесь не нужен.
                await using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = sql;
                    cmd.CommandTimeout = 120;
                    await cmd.ExecuteNonQueryAsync(ct);
                }

                await using (var markApplied = connection.CreateCommand())
                {
                    markApplied.Transaction = transaction;
                    markApplied.CommandText = "INSERT INTO __SchemaMigrations (FileName) VALUES (@FileName)";
                    markApplied.Parameters.AddWithValue("@FileName", fileName);
                    await markApplied.ExecuteNonQueryAsync(ct);
                }

                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
    }
}
