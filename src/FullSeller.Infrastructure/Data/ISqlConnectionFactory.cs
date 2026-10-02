using Npgsql;

namespace FullSeller.Infrastructure.Data;

public interface ISqlConnectionFactory
{
    Task<NpgsqlConnection> CreateOpenConnectionAsync(CancellationToken ct = default);
}
