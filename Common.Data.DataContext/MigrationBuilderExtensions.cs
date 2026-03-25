using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Common.Data.DataContext
{
    public static class MigrationBuilderExtensions
    {
        public static bool TableExists<TDbContext>(
            this MigrationBuilder migrationBuilder,
            string tableName,
            string? schemaName = null)
            where TDbContext : DbContext
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name must be provided.", nameof(tableName));

            var schema = schemaName ?? "dbo";

            const string sql = """
                SELECT COUNT(OBJECT_ID(@schemaName + '.' + @tableName, 'U')) AS Value
                """;

            var parameters = new DbParameter[]
            {
                new SqlParameter(nameof(tableName), tableName),
                new SqlParameter(nameof(schemaName), schema),
            };

            return ExecuteExistsQuery<TDbContext>(sql, parameters);
        }

        public static bool ColumnExists<TDbContext>(
            this MigrationBuilder migrationBuilder,
            string columnName,
            string tableName,
            string? schemaName = null)
            where TDbContext : DbContext
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentException("Column name must be provided.", nameof(columnName));
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name must be provided.", nameof(tableName));

            var schema = schemaName ?? "dbo";

            const string sql = """
                SELECT COUNT(*) AS Value
                FROM sys.columns
                WHERE object_id = OBJECT_ID(@schemaName + '.' + @tableName)
                  AND name = @columnName
                """;

            var parameters = new DbParameter[]
            {
                new SqlParameter(nameof(tableName), tableName),
                new SqlParameter(nameof(schemaName), schema),
                new SqlParameter(nameof(columnName), columnName),
            };

            return ExecuteExistsQuery<TDbContext>(sql, parameters);
        }

        public static bool IndexExists<TDbContext>(
            this MigrationBuilder migrationBuilder,
            string indexName,
            string tableName,
            string? schemaName = null)
            where TDbContext : DbContext
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("Index name must be provided.", nameof(indexName));
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name must be provided.", nameof(tableName));

            var schema = schemaName ?? "dbo";

            const string sql = """
                SELECT COUNT(*) AS Value
                FROM sys.indexes
                WHERE object_id = OBJECT_ID(@schemaName + '.' + @tableName)
                  AND name = @indexName
                """;

            var parameters = new DbParameter[]
            {
                new SqlParameter(nameof(tableName), tableName),
                new SqlParameter(nameof(schemaName), schema),
                new SqlParameter(nameof(indexName), indexName),
            };

            return ExecuteExistsQuery<TDbContext>(sql, parameters);
        }

        public static bool ForeignKeyExists<TDbContext>(
            this MigrationBuilder migrationBuilder,
            string keyName,
            string tableName,
            string? schemaName = null)
            where TDbContext : DbContext
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);
            if (string.IsNullOrWhiteSpace(keyName))
                throw new ArgumentException("Key name must be provided.", nameof(keyName));
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name must be provided.", nameof(tableName));

            var schema = schemaName ?? "dbo";

            const string sql = """
                SELECT COUNT(*) AS Value
                FROM sys.foreign_keys
                WHERE object_id = OBJECT_ID(@keyName)
                  AND parent_object_id = OBJECT_ID(@schemaName + '.' + @tableName)
                """;

            var parameters = new DbParameter[]
            {
                new SqlParameter(nameof(tableName), tableName),
                new SqlParameter(nameof(schemaName), schema),
                new SqlParameter(nameof(keyName), keyName),
            };

            return ExecuteExistsQuery<TDbContext>(sql, parameters);
        }

        private static bool ExecuteExistsQuery<TDbContext>(
            string sql,
            DbParameter[] parameters)
            where TDbContext : DbContext
        {
            var context = ServiceLocator.GetInstance<TDbContext>();

            if (!context.Database.CanConnect())
            {
                return false;
            }

            return context.Database
                .SqlQueryRaw<int>(sql, parameters)
                .Any(x => x > 0);
        }
    }
}
