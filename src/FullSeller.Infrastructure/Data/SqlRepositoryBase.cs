using Npgsql;

namespace FullSeller.Infrastructure.Data;

/// <summary>
/// Общий фундамент для всех ADO.NET-репозиториев. Поддерживает два режима:
///  1) автономный — каждый вызов открывает и закрывает своё соединение (по умолчанию, для чтения);
///  2) общий — соединение и транзакция переданы снаружи (используется <see cref="UnitOfWork"/>
///     для атомарных операций, например оформления заказа).
/// </summary>
public abstract class SqlRepositoryBase
{
    private readonly ISqlConnectionFactory? _factory;
    private readonly NpgsqlConnection? _sharedConnection;
    private readonly NpgsqlTransaction? _sharedTransaction;

    protected SqlRepositoryBase(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }

    protected SqlRepositoryBase(NpgsqlConnection sharedConnection, NpgsqlTransaction? sharedTransaction)
    {
        _sharedConnection = sharedConnection;
        _sharedTransaction = sharedTransaction;
    }

    /// <summary>Выполняет делегат с открытым соединением (и транзакцией, если она есть).
    /// В автономном режиме соединение закрывается по завершении.</summary>
    protected async Task<T> RunAsync<T>(Func<NpgsqlConnection, NpgsqlTransaction?, Task<T>> work, CancellationToken ct)
    {
        if (_sharedConnection is not null)
        {
            return await work(_sharedConnection, _sharedTransaction);
        }

        if (_factory is null)
            throw new InvalidOperationException("Репозиторий создан без фабрики соединений и без общего соединения.");

        await using var connection = await _factory.CreateOpenConnectionAsync(ct);
        return await work(connection, null);
    }

    protected Task RunAsync(Func<NpgsqlConnection, NpgsqlTransaction?, Task> work, CancellationToken ct) =>
        RunAsync(async (c, t) => { await work(c, t); return true; }, ct);

    protected static NpgsqlCommand CreateCommand(NpgsqlConnection connection, NpgsqlTransaction? transaction, string sql)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        if (transaction is not null) cmd.Transaction = transaction;
        return cmd;
    }
}
