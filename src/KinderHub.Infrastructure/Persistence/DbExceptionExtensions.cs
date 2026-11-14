using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace KinderHub.Infrastructure.Persistence;

public static class DbExceptionExtensions
{
    // SQLITE_CONSTRAINT = 19 → for example our unique index "one record per child per day"
    public static bool IsUniqueConstraintViolation(this DbUpdateException ex) =>
        ex.InnerException is SqliteException { SqliteErrorCode: 19 };
}
