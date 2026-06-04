using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.DataContext
{
    public static class DbContextExtensions
    {
        public static async Task EnsureSqlServerDatabaseCreatedAsync(this DbContext context)
        {
            if (!context.Database.IsRelational())
                return;

            var connectionString = context.Database.GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
                return;

            var builder = new SqlConnectionStringBuilder(connectionString);
            var dbName = builder.InitialCatalog;
            if (string.IsNullOrEmpty(dbName))
                return;

            builder.InitialCatalog = "master";

            await using var conn = new SqlConnection(builder.ConnectionString);
            await conn.OpenAsync();

            if (await DatabaseExistsAsync(conn, dbName))
                return;

            await using var createCmd = conn.CreateCommand();
            createCmd.CommandText = $"CREATE DATABASE [{dbName.Replace("]", "]]")}]";
            try
            {
                await createCmd.ExecuteNonQueryAsync();
            }
            catch (SqlException)
            {
            }
        }

        /// <summary>
        /// Calls EnsureCreated() and silently handles the race where a concurrent service
        /// already started creating the schema (SQL error 2714 — object already exists).
        /// </summary>
        public static void EnsureSchemaCreated(this DbContext context)
        {
            try
            {
                context.Database.EnsureCreated();
            }
            catch (SqlException ex) when (ex.Number == 2714)
            {
                // Concurrent service already created the schema — that's fine.
            }
        }

        private static async Task<bool> DatabaseExistsAsync(SqlConnection conn, string dbName)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1 FROM sys.databases WHERE name = @name";
            cmd.Parameters.AddWithValue("@name", dbName);
            var result = await cmd.ExecuteScalarAsync();
            return result is not null;
        }
    }
}
