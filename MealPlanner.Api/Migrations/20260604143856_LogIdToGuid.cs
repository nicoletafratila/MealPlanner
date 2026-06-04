using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class LogIdToGuid : Migration
    {
        // Switches Logs.Id from int IDENTITY to uniqueidentifier.
        // No other table has a foreign key referencing Logs.Id, so only the PK needs to be rebuilt.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add the new Guid column with NEWID() default so existing rows get GUIDs.
            migrationBuilder.Sql(
                "ALTER TABLE [Logs] ADD [NewId] uniqueidentifier NOT NULL " +
                "CONSTRAINT [DF_Logs_NewId] DEFAULT NEWID();");

            // 2. Drop the existing primary key.
            DropPrimaryKey(migrationBuilder, "Logs");

            // 3. Drop the old int identity column and remove the temporary DEFAULT.
            migrationBuilder.Sql("ALTER TABLE [Logs] DROP CONSTRAINT [DF_Logs_NewId];");
            migrationBuilder.Sql("ALTER TABLE [Logs] DROP COLUMN [Id];");

            // 4. Rename the new column to Id.
            migrationBuilder.Sql("EXEC sp_rename N'[Logs].[NewId]', N'Id', N'COLUMN';");

            // 5. Recreate the primary key.
            migrationBuilder.Sql(
                "ALTER TABLE [Logs] ADD CONSTRAINT [PK_Logs] PRIMARY KEY ([Id]);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverts to int IDENTITY. Original int ids are not recoverable; a fresh sequence is assigned.
            migrationBuilder.Sql(
                "ALTER TABLE [Logs] ADD [NewId] int IDENTITY(1,1) NOT NULL;");

            DropPrimaryKey(migrationBuilder, "Logs");

            migrationBuilder.Sql("ALTER TABLE [Logs] DROP COLUMN [Id];");

            migrationBuilder.Sql("EXEC sp_rename N'[Logs].[NewId]', N'Id', N'COLUMN';");

            migrationBuilder.Sql(
                "ALTER TABLE [Logs] ADD CONSTRAINT [PK_Logs] PRIMARY KEY ([Id]);");
        }

        private static void DropPrimaryKey(MigrationBuilder migrationBuilder, string table)
        {
            migrationBuilder.Sql(
                "DECLARE @pk sysname; " +
                "SELECT @pk = [name] FROM sys.key_constraints " +
                "WHERE parent_object_id = (SELECT object_id FROM sys.objects WHERE name = N'" + table + "' AND type = N'U') " +
                "AND [type] = 'PK'; " +
                "IF @pk IS NOT NULL EXEC(N'ALTER TABLE [" + table + "] DROP CONSTRAINT [' + @pk + N']');");
        }
    }
}
