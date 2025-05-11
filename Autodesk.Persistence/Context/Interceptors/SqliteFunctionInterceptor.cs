using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Data;

namespace Autodesk.Persistence.Context.Interceptors
{
    public class SqliteFunctionInterceptor : DbConnectionInterceptor
    {
        public override void ConnectionOpened(
        DbConnection connection,
        ConnectionEndEventData eventData)
        {
            if (connection is SqliteConnection sqlite)
                RegisterFunction(sqlite);

            base.ConnectionOpened(connection, eventData);
        }

        public override async Task ConnectionOpenedAsync(
            DbConnection connection,
            ConnectionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            if (connection is SqliteConnection sqlite)
                RegisterFunction(sqlite);

            await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
        }

        private void RegisterFunction(SqliteConnection sqlite)
        {
            // Registra siempre, esté abierta o no
            sqlite.CreateFunction<string, string, int>(
              "StringCompareOrdinal",
              (a, b) => a == b ? 0
                        : string.Compare(a, b, StringComparison.Ordinal) > 0 ? 1
                        : -1
            );
        }
    }
}
