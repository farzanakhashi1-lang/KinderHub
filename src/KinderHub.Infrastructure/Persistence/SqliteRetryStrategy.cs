using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Storage;

namespace KinderHub.Infrastructure.Persistence;

// Resilience: if the database file is busy for a moment (another save is running),
// Entity Framework waits a little and tries again (up to 3 times) instead of failing.
public class SqliteRetryStrategy(ExecutionStrategyDependencies dependencies)
    : ExecutionStrategy(dependencies, 3, TimeSpan.FromSeconds(2))
{
    // SQLITE_BUSY = 5, SQLITE_LOCKED = 6 → temporary problems, safe to retry
    protected override bool ShouldRetryOn(Exception exception) =>
        exception is SqliteException { SqliteErrorCode: 5 or 6 };
}
