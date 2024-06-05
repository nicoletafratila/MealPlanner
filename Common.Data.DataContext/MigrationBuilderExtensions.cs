using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Common.Data.DataContext
{
    public static class MigrationBuilderExtensions
    {
        public static bool TableExists<TDbContext>(this MigrationBuilder migrationBuilder, string tableName, string? schemaName = null) where TDbContext : DbContext
        {
            TDbContext instance = ServiceLocator.Current.GetInstance<TDbContext>();
            if (!instance.Database.CanConnect())
            {
                return false;
            }

            return instance.Database.SqlQueryRaw<int>("SELECT COUNT(OBJECT_ID(@schemaName + '.' + @tableName, 'U')) AS Value", new DbParameter[2]
            {
                new SqlParameter(nameof(tableName), tableName),
                new SqlParameter(nameof(schemaName), schemaName ?? "dbo"),
            }).Any((int x) => x > 0);
        }

        public static bool ColumnExists<TDbContext>(this MigrationBuilder migrationBuilder, string columnName, string tableName, string? schemaName = null) where TDbContext : DbContext
        {
            TDbContext instance = ServiceLocator.Current.GetInstance<TDbContext>();
            if (!instance.Database.CanConnect())
            {
                return false;
            }

            return instance.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM sys.columns WHERE object_id = OBJECT_ID(@schemaName + '.' + @tableName) AND name = @columnName", new DbParameter[3]
            {
                new SqlParameter(nameof(tableName), tableName),
                new SqlParameter(nameof(schemaName), schemaName ?? "dbo"),
                new SqlParameter(nameof(columnName), columnName),
            }).Any((int x) => x > 0);
        }

        public static bool IndexExists<TDbContext>(this MigrationBuilder migrationBuilder, string indexName, string tableName, string? schemaName = null) where TDbContext : DbContext
        {
            TDbContext instance = ServiceLocator.Current.GetInstance<TDbContext>();
            if (!instance.Database.CanConnect())
            {
                return false;
            }

            return instance.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM sys.indexes WHERE object_id = OBJECT_ID(@schemaName + '.' + @tableName) AND name = @indexName", new DbParameter[3]
            {
                new SqlParameter(nameof(tableName), tableName),
                new SqlParameter(nameof(schemaName), schemaName ?? "dbo"),
                new SqlParameter(nameof(indexName), indexName),
            }).Any((int x) => x > 0);
        }

        public static bool ForeignKeyExists<TDbContext>(this MigrationBuilder migrationBuilder, string keyName, string tableName, string? schemaName = null) where TDbContext : DbContext
        {
            TDbContext instance = ServiceLocator.Current.GetInstance<TDbContext>();
            if (!instance.Database.CanConnect())
            {
                return false;
            }

            return instance.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM sys.foreign_keys WHERE object_id = OBJECT_ID(@keyName) AND parent_object_id = OBJECT_ID(@schemaName + '.' + @tableName)", new DbParameter[3]
            {
                new SqlParameter(nameof(tableName), tableName),
                new SqlParameter(nameof(schemaName), schemaName ?? "dbo"),
                new SqlParameter(nameof(keyName), keyName),
            }).Any((int x) => x > 0);
        }
    }
}
