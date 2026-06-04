using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class UnitIdToGuid : Migration
    {
        // Switches Units.Id (and the Products.BaseUnitId, RecipeIngredients.UnitId,
        // ShoppingListProducts.UnitId FKs) from int IDENTITY to uniqueidentifier,
        // preserving all existing rows and child-row linkage. EF's scaffolded AlterColumn
        // cannot convert int -> uniqueidentifier in place, so this is hand-written:
        // add new Guid columns, backfill from the int mapping, drop/rename the old columns,
        // then recreate keys, indexes and foreign keys against the new Guid columns.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add the new Guid column to Units. A temporary DEFAULT NEWID() assigns a
            //    distinct Guid to every existing row automatically.
            migrationBuilder.Sql(
                "ALTER TABLE [Units] ADD [NewId] uniqueidentifier NOT NULL " +
                "CONSTRAINT [DF_Units_NewId] DEFAULT NEWID();");

            // 2. Add nullable new Guid FK columns on each referencing table.
            migrationBuilder.Sql(
                "ALTER TABLE [Products] ADD [NewBaseUnitId] uniqueidentifier NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeIngredients] ADD [NewUnitId] uniqueidentifier NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingListProducts] ADD [NewUnitId] uniqueidentifier NULL;");

            // 3. Backfill each FK column by joining on the old int id.
            migrationBuilder.Sql(
                "UPDATE p SET p.[NewBaseUnitId] = u.[NewId] " +
                "FROM [Products] p INNER JOIN [Units] u ON u.[Id] = p.[BaseUnitId];");
            migrationBuilder.Sql(
                "UPDATE ri SET ri.[NewUnitId] = u.[NewId] " +
                "FROM [RecipeIngredients] ri INNER JOIN [Units] u ON u.[Id] = ri.[UnitId];");
            migrationBuilder.Sql(
                "UPDATE slp SET slp.[NewUnitId] = u.[NewId] " +
                "FROM [ShoppingListProducts] slp INNER JOIN [Units] u ON u.[Id] = slp.[UnitId];");

            // 4. Remove orphaned child rows (those whose FK could not be mapped).
            migrationBuilder.Sql(
                "DELETE FROM [RecipeIngredients] WHERE [NewUnitId] IS NULL;");
            migrationBuilder.Sql(
                "DELETE FROM [ShoppingListProducts] WHERE [NewUnitId] IS NULL;");
            migrationBuilder.Sql(
                "DELETE FROM [Products] WHERE [NewBaseUnitId] IS NULL;");

            // 5. Drop all foreign keys referencing Units.Id.
            DropForeignKeysReferencing(migrationBuilder, "Products",             "Units");
            DropForeignKeysReferencing(migrationBuilder, "RecipeIngredients",    "Units");
            DropForeignKeysReferencing(migrationBuilder, "ShoppingListProducts", "Units");

            // 6. Drop indexes on the old int FK columns.
            DropIndexesOnColumn(migrationBuilder, "Products",             "BaseUnitId");
            DropIndexesOnColumn(migrationBuilder, "RecipeIngredients",    "UnitId");
            DropIndexesOnColumn(migrationBuilder, "ShoppingListProducts", "UnitId");

            // 7. Drop the primary key of Units (tied to the int Id column).
            DropPrimaryKey(migrationBuilder, "Units");

            // 7b. Drop any system-generated DEFAULT constraints on the old FK columns.
            //     These are created automatically when a NOT NULL column is added with a default
            //     value (e.g. 0 for int), and must be removed before the column can be dropped.
            DropDefaultConstraints(migrationBuilder, "Products",             "BaseUnitId");
            DropDefaultConstraints(migrationBuilder, "RecipeIngredients",    "UnitId");
            DropDefaultConstraints(migrationBuilder, "ShoppingListProducts", "UnitId");

            // 8. Drop the old int columns and remove the temporary DEFAULT on Units.NewId.
            migrationBuilder.Sql("ALTER TABLE [Products]             DROP COLUMN [BaseUnitId];");
            migrationBuilder.Sql("ALTER TABLE [RecipeIngredients]    DROP COLUMN [UnitId];");
            migrationBuilder.Sql("ALTER TABLE [ShoppingListProducts] DROP COLUMN [UnitId];");
            migrationBuilder.Sql("ALTER TABLE [Units] DROP CONSTRAINT [DF_Units_NewId];");
            migrationBuilder.Sql("ALTER TABLE [Units] DROP COLUMN [Id];");

            // 9. Rename new columns to the original names.
            migrationBuilder.Sql("EXEC sp_rename N'[Units].[NewId]',                    N'Id',         N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[Products].[NewBaseUnitId]',         N'BaseUnitId', N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[RecipeIngredients].[NewUnitId]',    N'UnitId',     N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[ShoppingListProducts].[NewUnitId]', N'UnitId',     N'COLUMN';");

            // 10. Make the FK columns NOT NULL now that every row has been populated.
            migrationBuilder.Sql("ALTER TABLE [Products]             ALTER COLUMN [BaseUnitId] uniqueidentifier NOT NULL;");
            migrationBuilder.Sql("ALTER TABLE [RecipeIngredients]    ALTER COLUMN [UnitId]     uniqueidentifier NOT NULL;");
            migrationBuilder.Sql("ALTER TABLE [ShoppingListProducts] ALTER COLUMN [UnitId]     uniqueidentifier NOT NULL;");

            // 11. Recreate the primary key, indexes and foreign keys.
            migrationBuilder.Sql(
                "ALTER TABLE [Units] ADD CONSTRAINT [PK_Units] PRIMARY KEY ([Id]);");
            migrationBuilder.Sql(
                "CREATE INDEX [IX_Products_BaseUnitId]         ON [Products]             ([BaseUnitId]);");
            migrationBuilder.Sql(
                "CREATE INDEX [IX_RecipeIngredients_UnitId]    ON [RecipeIngredients]    ([UnitId]);");
            migrationBuilder.Sql(
                "CREATE INDEX [IX_ShoppingListProducts_UnitId] ON [ShoppingListProducts] ([UnitId]);");
            migrationBuilder.Sql(
                "ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_Units_BaseUnitId] " +
                "FOREIGN KEY ([BaseUnitId]) REFERENCES [Units] ([Id]) ON DELETE CASCADE;");
            // NO ACTION to avoid multiple cascade paths:
            //   Units → Products.BaseUnitId (CASCADE) → RecipeIngredients.ProductId
            //   Units → RecipeIngredients.UnitId  — would form a second path to the same table.
            // Same reasoning applies to ShoppingListProducts.
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeIngredients] ADD CONSTRAINT [FK_RecipeIngredients_Units_UnitId] " +
                "FOREIGN KEY ([UnitId]) REFERENCES [Units] ([Id]) ON DELETE NO ACTION;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingListProducts] ADD CONSTRAINT [FK_ShoppingListProducts_Units_UnitId] " +
                "FOREIGN KEY ([UnitId]) REFERENCES [Units] ([Id]) ON DELETE NO ACTION;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverts to an int IDENTITY primary key. The original int ids are not
            // recoverable; a fresh identity sequence is assigned while preserving linkage.
            migrationBuilder.Sql(
                "ALTER TABLE [Units] ADD [NewId] int IDENTITY(1,1) NOT NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [Products]             ADD [NewBaseUnitId] int NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeIngredients]    ADD [NewUnitId]     int NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingListProducts] ADD [NewUnitId]     int NULL;");

            migrationBuilder.Sql(
                "UPDATE p SET p.[NewBaseUnitId] = u.[NewId] " +
                "FROM [Products] p INNER JOIN [Units] u ON u.[Id] = p.[BaseUnitId];");
            migrationBuilder.Sql(
                "UPDATE ri SET ri.[NewUnitId] = u.[NewId] " +
                "FROM [RecipeIngredients] ri INNER JOIN [Units] u ON u.[Id] = ri.[UnitId];");
            migrationBuilder.Sql(
                "UPDATE slp SET slp.[NewUnitId] = u.[NewId] " +
                "FROM [ShoppingListProducts] slp INNER JOIN [Units] u ON u.[Id] = slp.[UnitId];");

            DropForeignKeysReferencing(migrationBuilder, "Products",             "Units");
            DropForeignKeysReferencing(migrationBuilder, "RecipeIngredients",    "Units");
            DropForeignKeysReferencing(migrationBuilder, "ShoppingListProducts", "Units");

            DropIndexesOnColumn(migrationBuilder, "Products",             "BaseUnitId");
            DropIndexesOnColumn(migrationBuilder, "RecipeIngredients",    "UnitId");
            DropIndexesOnColumn(migrationBuilder, "ShoppingListProducts", "UnitId");

            DropPrimaryKey(migrationBuilder, "Units");

            DropDefaultConstraints(migrationBuilder, "Products",             "BaseUnitId");
            DropDefaultConstraints(migrationBuilder, "RecipeIngredients",    "UnitId");
            DropDefaultConstraints(migrationBuilder, "ShoppingListProducts", "UnitId");

            migrationBuilder.Sql("ALTER TABLE [Products]             DROP COLUMN [BaseUnitId];");
            migrationBuilder.Sql("ALTER TABLE [RecipeIngredients]    DROP COLUMN [UnitId];");
            migrationBuilder.Sql("ALTER TABLE [ShoppingListProducts] DROP COLUMN [UnitId];");
            migrationBuilder.Sql("ALTER TABLE [Units] DROP COLUMN [Id];");

            migrationBuilder.Sql("EXEC sp_rename N'[Units].[NewId]',                    N'Id',         N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[Products].[NewBaseUnitId]',         N'BaseUnitId', N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[RecipeIngredients].[NewUnitId]',    N'UnitId',     N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[ShoppingListProducts].[NewUnitId]', N'UnitId',     N'COLUMN';");

            migrationBuilder.Sql("ALTER TABLE [Products]             ALTER COLUMN [BaseUnitId] int NOT NULL;");
            migrationBuilder.Sql("ALTER TABLE [RecipeIngredients]    ALTER COLUMN [UnitId]     int NOT NULL;");
            migrationBuilder.Sql("ALTER TABLE [ShoppingListProducts] ALTER COLUMN [UnitId]     int NOT NULL;");

            migrationBuilder.Sql(
                "ALTER TABLE [Units] ADD CONSTRAINT [PK_Units] PRIMARY KEY ([Id]);");
            migrationBuilder.Sql(
                "CREATE INDEX [IX_Products_BaseUnitId]         ON [Products]             ([BaseUnitId]);");
            migrationBuilder.Sql(
                "CREATE INDEX [IX_RecipeIngredients_UnitId]    ON [RecipeIngredients]    ([UnitId]);");
            migrationBuilder.Sql(
                "CREATE INDEX [IX_ShoppingListProducts_UnitId] ON [ShoppingListProducts] ([UnitId]);");
            migrationBuilder.Sql(
                "ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_Units_BaseUnitId] " +
                "FOREIGN KEY ([BaseUnitId]) REFERENCES [Units] ([Id]) ON DELETE CASCADE;");
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeIngredients] ADD CONSTRAINT [FK_RecipeIngredients_Units_UnitId] " +
                "FOREIGN KEY ([UnitId]) REFERENCES [Units] ([Id]) ON DELETE NO ACTION;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingListProducts] ADD CONSTRAINT [FK_ShoppingListProducts_Units_UnitId] " +
                "FOREIGN KEY ([UnitId]) REFERENCES [Units] ([Id]) ON DELETE NO ACTION;");
        }

        // Drops ALL DEFAULT constraints on <column> in <table>. These are created when a
        // NOT NULL column is added to an existing table with a default value, and SQL Server
        // will not let you drop the column until the constraint is gone first.
        private static void DropDefaultConstraints(MigrationBuilder migrationBuilder, string table, string column)
        {
            migrationBuilder.Sql(
                "DECLARE @sql nvarchar(max) = N''; " +
                "SELECT @sql = @sql + N'ALTER TABLE [" + table + "] DROP CONSTRAINT [' + d.[name] + N']; ' " +
                "FROM sys.default_constraints d " +
                "INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id " +
                "WHERE d.parent_object_id = (SELECT object_id FROM sys.objects WHERE name = N'" + table + "' AND type = N'U') " +
                "AND c.[name] = N'" + column + "'; " +
                "IF LEN(@sql) > 0 EXEC(@sql);");
        }

        // Drops ALL foreign keys on <table> that reference <referencedTable>, looked up
        // by their actual names in the catalog. Uses sys.objects to avoid OBJECT_ID schema issues.
        private static void DropForeignKeysReferencing(MigrationBuilder migrationBuilder, string table, string referencedTable)
        {
            migrationBuilder.Sql(
                "DECLARE @sql nvarchar(max) = N''; " +
                "SELECT @sql = @sql + N'ALTER TABLE [" + table + "] DROP CONSTRAINT [' + [name] + N']; ' " +
                "FROM sys.foreign_keys " +
                "WHERE parent_object_id = (SELECT object_id FROM sys.objects WHERE name = N'" + table + "' AND type = N'U') " +
                "AND referenced_object_id = (SELECT object_id FROM sys.objects WHERE name = N'" + referencedTable + "' AND type = N'U'); " +
                "IF LEN(@sql) > 0 EXEC(@sql);");
        }

        // Drops the primary key of <table> by its actual name in the catalog.
        private static void DropPrimaryKey(MigrationBuilder migrationBuilder, string table)
        {
            migrationBuilder.Sql(
                "DECLARE @pk sysname; " +
                "SELECT @pk = [name] FROM sys.key_constraints " +
                "WHERE parent_object_id = (SELECT object_id FROM sys.objects WHERE name = N'" + table + "' AND type = N'U') " +
                "AND [type] = 'PK'; " +
                "IF @pk IS NOT NULL EXEC(N'ALTER TABLE [" + table + "] DROP CONSTRAINT [' + @pk + N']');");
        }

        // Drops ALL non-PK, non-unique indexes on <table> that include <column>.
        // Concatenates every matching DROP INDEX into a single batch so renamed columns
        // (which keep the old index name) are also caught alongside any newer indexes.
        private static void DropIndexesOnColumn(MigrationBuilder migrationBuilder, string table, string column)
        {
            migrationBuilder.Sql(
                "DECLARE @sql nvarchar(max) = N''; " +
                "SELECT @sql = @sql + N'DROP INDEX [' + i.[name] + N'] ON [" + table + "]; ' " +
                "FROM sys.indexes i " +
                "INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id " +
                "INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id " +
                "WHERE i.object_id = (SELECT object_id FROM sys.objects WHERE name = N'" + table + "' AND type = N'U') " +
                "AND c.[name] = N'" + column + "' " +
                "AND i.is_primary_key = 0 AND i.is_unique_constraint = 0; " +
                "IF LEN(@sql) > 0 EXEC(@sql);");
        }
    }
}
