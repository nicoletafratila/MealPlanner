using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class ProductIdToGuid : Migration
    {
        // Switches Products.Id (and the ShoppingListProducts.ProductId, RecipeIngredients.ProductId
        // FKs) from int IDENTITY to uniqueidentifier, preserving all existing rows and linkage.
        // EF's scaffolded AlterColumn cannot convert int -> uniqueidentifier in place, so this is
        // hand-written: add new Guid columns, backfill from the int mapping, drop/rename the old
        // columns, then recreate keys, indexes and foreign keys against the new Guid columns.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add the new Guid column to Products with NEWID() default so existing rows get GUIDs.
            migrationBuilder.Sql(
                "ALTER TABLE [Products] ADD [NewId] uniqueidentifier NOT NULL " +
                "CONSTRAINT [DF_Products_NewId] DEFAULT NEWID();");

            // 2. Add nullable new Guid FK columns on each referencing table.
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeIngredients]    ADD [NewProductId] uniqueidentifier NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingListProducts] ADD [NewProductId] uniqueidentifier NULL;");

            // 3. Backfill each FK column by joining on the old int id.
            migrationBuilder.Sql(
                "UPDATE ri SET ri.[NewProductId] = p.[NewId] " +
                "FROM [RecipeIngredients] ri INNER JOIN [Products] p ON p.[Id] = ri.[ProductId];");
            migrationBuilder.Sql(
                "UPDATE slp SET slp.[NewProductId] = p.[NewId] " +
                "FROM [ShoppingListProducts] slp INNER JOIN [Products] p ON p.[Id] = slp.[ProductId];");

            // 4. Remove orphaned child rows.
            migrationBuilder.Sql("DELETE FROM [RecipeIngredients]    WHERE [NewProductId] IS NULL;");
            migrationBuilder.Sql("DELETE FROM [ShoppingListProducts] WHERE [NewProductId] IS NULL;");

            // 5. Drop all foreign keys referencing Products.Id.
            DropForeignKeysReferencing(migrationBuilder, "RecipeIngredients",    "Products");
            DropForeignKeysReferencing(migrationBuilder, "ShoppingListProducts", "Products");

            // 6. Drop indexes on the old int FK columns (non-PK/non-unique only).
            DropIndexesOnColumn(migrationBuilder, "RecipeIngredients",    "ProductId");
            DropIndexesOnColumn(migrationBuilder, "ShoppingListProducts", "ProductId");

            // 7. Drop composite primary keys of RecipeIngredients and ShoppingListProducts
            //    — both include ProductId so the column cannot be dropped while the PK exists.
            DropPrimaryKey(migrationBuilder, "RecipeIngredients");
            DropPrimaryKey(migrationBuilder, "ShoppingListProducts");

            // 7b. Drop the primary key of Products.
            DropPrimaryKey(migrationBuilder, "Products");

            // 7c. Drop any system-generated DEFAULT constraints on the old FK columns.
            DropDefaultConstraints(migrationBuilder, "RecipeIngredients",    "ProductId");
            DropDefaultConstraints(migrationBuilder, "ShoppingListProducts", "ProductId");

            // 8. Drop the old int columns and remove the temporary DEFAULT on Products.NewId.
            migrationBuilder.Sql("ALTER TABLE [RecipeIngredients]    DROP COLUMN [ProductId];");
            migrationBuilder.Sql("ALTER TABLE [ShoppingListProducts] DROP COLUMN [ProductId];");
            migrationBuilder.Sql("ALTER TABLE [Products] DROP CONSTRAINT [DF_Products_NewId];");
            migrationBuilder.Sql("ALTER TABLE [Products] DROP COLUMN [Id];");

            // 9. Rename new columns to the original names.
            migrationBuilder.Sql("EXEC sp_rename N'[Products].[NewId]',                    N'Id',        N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[RecipeIngredients].[NewProductId]',    N'ProductId', N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[ShoppingListProducts].[NewProductId]', N'ProductId', N'COLUMN';");

            // 10. Make the FK columns NOT NULL.
            migrationBuilder.Sql("ALTER TABLE [RecipeIngredients]    ALTER COLUMN [ProductId] uniqueidentifier NOT NULL;");
            migrationBuilder.Sql("ALTER TABLE [ShoppingListProducts] ALTER COLUMN [ProductId] uniqueidentifier NOT NULL;");

            // 11. Recreate the primary keys, indexes and foreign keys.
            migrationBuilder.Sql(
                "ALTER TABLE [Products] ADD CONSTRAINT [PK_Products] PRIMARY KEY ([Id]);");
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeIngredients] ADD CONSTRAINT [PK_RecipeIngredients] " +
                "PRIMARY KEY ([RecipeId], [ProductId]);");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingListProducts] ADD CONSTRAINT [PK_ShoppingListProducts] " +
                "PRIMARY KEY ([ShoppingListId], [ProductId]);");
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeIngredients] ADD CONSTRAINT [FK_RecipeIngredients_Products_ProductId] " +
                "FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE;");
            // NO ACTION to avoid multiple cascade paths:
            //   Products -> ShoppingListProducts.ProductId
            //   Products -> RecipeIngredients -> (shopping list products via mealplan) — potential second path
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingListProducts] ADD CONSTRAINT [FK_ShoppingListProducts_Products_ProductId] " +
                "FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverts to int IDENTITY. Original int ids are not recoverable; a fresh sequence is assigned.
            migrationBuilder.Sql(
                "ALTER TABLE [Products] ADD [NewId] int IDENTITY(1,1) NOT NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeIngredients]    ADD [NewProductId] int NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingListProducts] ADD [NewProductId] int NULL;");

            migrationBuilder.Sql(
                "UPDATE ri SET ri.[NewProductId] = p.[NewId] " +
                "FROM [RecipeIngredients] ri INNER JOIN [Products] p ON p.[Id] = ri.[ProductId];");
            migrationBuilder.Sql(
                "UPDATE slp SET slp.[NewProductId] = p.[NewId] " +
                "FROM [ShoppingListProducts] slp INNER JOIN [Products] p ON p.[Id] = slp.[ProductId];");

            DropForeignKeysReferencing(migrationBuilder, "RecipeIngredients",    "Products");
            DropForeignKeysReferencing(migrationBuilder, "ShoppingListProducts", "Products");

            DropIndexesOnColumn(migrationBuilder, "RecipeIngredients",    "ProductId");
            DropIndexesOnColumn(migrationBuilder, "ShoppingListProducts", "ProductId");

            DropPrimaryKey(migrationBuilder, "RecipeIngredients");
            DropPrimaryKey(migrationBuilder, "ShoppingListProducts");
            DropPrimaryKey(migrationBuilder, "Products");

            DropDefaultConstraints(migrationBuilder, "RecipeIngredients",    "ProductId");
            DropDefaultConstraints(migrationBuilder, "ShoppingListProducts", "ProductId");

            migrationBuilder.Sql("ALTER TABLE [RecipeIngredients]    DROP COLUMN [ProductId];");
            migrationBuilder.Sql("ALTER TABLE [ShoppingListProducts] DROP COLUMN [ProductId];");
            migrationBuilder.Sql("ALTER TABLE [Products] DROP COLUMN [Id];");

            migrationBuilder.Sql("EXEC sp_rename N'[Products].[NewId]',                    N'Id',        N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[RecipeIngredients].[NewProductId]',    N'ProductId', N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[ShoppingListProducts].[NewProductId]', N'ProductId', N'COLUMN';");

            migrationBuilder.Sql("ALTER TABLE [RecipeIngredients]    ALTER COLUMN [ProductId] int NOT NULL;");
            migrationBuilder.Sql("ALTER TABLE [ShoppingListProducts] ALTER COLUMN [ProductId] int NOT NULL;");

            migrationBuilder.Sql(
                "ALTER TABLE [Products] ADD CONSTRAINT [PK_Products] PRIMARY KEY ([Id]);");
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeIngredients] ADD CONSTRAINT [PK_RecipeIngredients] " +
                "PRIMARY KEY ([RecipeId], [ProductId]);");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingListProducts] ADD CONSTRAINT [PK_ShoppingListProducts] " +
                "PRIMARY KEY ([ShoppingListId], [ProductId]);");
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeIngredients] ADD CONSTRAINT [FK_RecipeIngredients_Products_ProductId] " +
                "FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingListProducts] ADD CONSTRAINT [FK_ShoppingListProducts_Products_ProductId] " +
                "FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION;");
        }

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

        private static void DropPrimaryKey(MigrationBuilder migrationBuilder, string table)
        {
            migrationBuilder.Sql(
                "DECLARE @pk sysname; " +
                "SELECT @pk = [name] FROM sys.key_constraints " +
                "WHERE parent_object_id = (SELECT object_id FROM sys.objects WHERE name = N'" + table + "' AND type = N'U') " +
                "AND [type] = 'PK'; " +
                "IF @pk IS NOT NULL EXEC(N'ALTER TABLE [" + table + "] DROP CONSTRAINT [' + @pk + N']');");
        }

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
