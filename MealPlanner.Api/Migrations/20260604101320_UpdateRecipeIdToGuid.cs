using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRecipeIdToGuid : Migration
    {
        // Switches Recipes.Id (and the RecipeIngredients.RecipeId, MealPlanRecipes.RecipeId FKs)
        // from int IDENTITY to uniqueidentifier, preserving all existing rows and child-row linkage.
        // EF's scaffolded AlterColumn cannot convert int -> uniqueidentifier in place, so this is
        // hand-written: add new Guid columns, backfill from the int mapping, drop/rename the old
        // columns, then recreate keys and foreign keys against the new Guid columns.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add the new Guid column to Recipes. A temporary DEFAULT NEWID() assigns a
            //    distinct Guid to every existing row automatically.
            migrationBuilder.Sql(
                "ALTER TABLE [Recipes] ADD [NewId] uniqueidentifier NOT NULL " +
                "CONSTRAINT [DF_Recipes_NewId] DEFAULT NEWID();");

            // 2. Add nullable new Guid FK columns on each referencing table.
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeIngredients] ADD [NewRecipeId] uniqueidentifier NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] ADD [NewRecipeId] uniqueidentifier NULL;");

            // 3. Backfill each FK column by joining on the old int id.
            migrationBuilder.Sql(
                "UPDATE ri SET ri.[NewRecipeId] = r.[NewId] " +
                "FROM [RecipeIngredients] ri INNER JOIN [Recipes] r ON r.[Id] = ri.[RecipeId];");
            migrationBuilder.Sql(
                "UPDATE mpr SET mpr.[NewRecipeId] = r.[NewId] " +
                "FROM [MealPlanRecipes] mpr INNER JOIN [Recipes] r ON r.[Id] = mpr.[RecipeId];");

            // 4. Remove orphaned child rows (those whose FK could not be mapped).
            migrationBuilder.Sql(
                "DELETE FROM [RecipeIngredients] WHERE [NewRecipeId] IS NULL;");
            migrationBuilder.Sql(
                "DELETE FROM [MealPlanRecipes] WHERE [NewRecipeId] IS NULL;");

            // 5. Drop foreign keys referencing Recipes.Id.
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeIngredients] DROP CONSTRAINT [FK_RecipeIngredients_Recipes_RecipeId];");
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] DROP CONSTRAINT [FK_MealPlanRecipes_Recipes_RecipeId];");

            // 6. Drop indexes on the old int RecipeId FK columns.
            DropIndexesOnColumn(migrationBuilder, "RecipeIngredients", "RecipeId");
            DropIndexesOnColumn(migrationBuilder, "MealPlanRecipes",   "RecipeId");

            // 7. Drop the composite primary keys that include the old int RecipeId column.
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeIngredients] DROP CONSTRAINT [PK_RecipeIngredients];");
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] DROP CONSTRAINT [PK_MealPlanRecipes];");

            // 8. Drop the primary key of Recipes (tied to the int Id column).
            DropPrimaryKey(migrationBuilder, "Recipes");

            // 9. Drop any system-generated DEFAULT constraints on the old FK columns before dropping them.
            DropDefaultConstraints(migrationBuilder, "RecipeIngredients", "RecipeId");
            DropDefaultConstraints(migrationBuilder, "MealPlanRecipes",   "RecipeId");

            // 10. Drop the old int columns and remove the temporary DEFAULT on Recipes.NewId.
            migrationBuilder.Sql("ALTER TABLE [RecipeIngredients] DROP COLUMN [RecipeId];");
            migrationBuilder.Sql("ALTER TABLE [MealPlanRecipes]   DROP COLUMN [RecipeId];");
            migrationBuilder.Sql("ALTER TABLE [Recipes] DROP CONSTRAINT [DF_Recipes_NewId];");
            migrationBuilder.Sql("ALTER TABLE [Recipes] DROP COLUMN [Id];");

            // 11. Rename new columns to the original names.
            migrationBuilder.Sql("EXEC sp_rename N'[Recipes].[NewId]',                  N'Id',       N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[RecipeIngredients].[NewRecipeId]', N'RecipeId', N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[MealPlanRecipes].[NewRecipeId]',   N'RecipeId', N'COLUMN';");

            // 12. Make the FK columns NOT NULL now that every row has been populated.
            migrationBuilder.Sql("ALTER TABLE [RecipeIngredients] ALTER COLUMN [RecipeId] uniqueidentifier NOT NULL;");
            migrationBuilder.Sql("ALTER TABLE [MealPlanRecipes]   ALTER COLUMN [RecipeId] uniqueidentifier NOT NULL;");

            // 13. Recreate the primary key, composite keys, and foreign keys.
            migrationBuilder.Sql(
                "ALTER TABLE [Recipes] ADD CONSTRAINT [PK_Recipes] PRIMARY KEY ([Id]);");
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeIngredients] ADD CONSTRAINT [PK_RecipeIngredients] " +
                "PRIMARY KEY ([RecipeId], [ProductId]);");
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] ADD CONSTRAINT [PK_MealPlanRecipes] " +
                "PRIMARY KEY ([MealPlanId], [RecipeId]);");
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeIngredients] ADD CONSTRAINT [FK_RecipeIngredients_Recipes_RecipeId] " +
                "FOREIGN KEY ([RecipeId]) REFERENCES [Recipes] ([Id]) ON DELETE CASCADE;");
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] ADD CONSTRAINT [FK_MealPlanRecipes_Recipes_RecipeId] " +
                "FOREIGN KEY ([RecipeId]) REFERENCES [Recipes] ([Id]) ON DELETE NO ACTION;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverts to an int IDENTITY primary key. The original int ids are not recoverable;
            // a fresh identity sequence is assigned while preserving child-table linkage.
            migrationBuilder.Sql(
                "ALTER TABLE [Recipes] ADD [NewId] int IDENTITY(1,1) NOT NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeIngredients] ADD [NewRecipeId] int NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes]   ADD [NewRecipeId] int NULL;");

            migrationBuilder.Sql(
                "UPDATE ri SET ri.[NewRecipeId] = r.[NewId] " +
                "FROM [RecipeIngredients] ri INNER JOIN [Recipes] r ON r.[Id] = ri.[RecipeId];");
            migrationBuilder.Sql(
                "UPDATE mpr SET mpr.[NewRecipeId] = r.[NewId] " +
                "FROM [MealPlanRecipes] mpr INNER JOIN [Recipes] r ON r.[Id] = mpr.[RecipeId];");

            migrationBuilder.Sql(
                "ALTER TABLE [RecipeIngredients] DROP CONSTRAINT [FK_RecipeIngredients_Recipes_RecipeId];");
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] DROP CONSTRAINT [FK_MealPlanRecipes_Recipes_RecipeId];");

            DropIndexesOnColumn(migrationBuilder, "RecipeIngredients", "RecipeId");
            DropIndexesOnColumn(migrationBuilder, "MealPlanRecipes",   "RecipeId");

            migrationBuilder.Sql(
                "ALTER TABLE [RecipeIngredients] DROP CONSTRAINT [PK_RecipeIngredients];");
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] DROP CONSTRAINT [PK_MealPlanRecipes];");

            DropPrimaryKey(migrationBuilder, "Recipes");

            DropDefaultConstraints(migrationBuilder, "RecipeIngredients", "RecipeId");
            DropDefaultConstraints(migrationBuilder, "MealPlanRecipes",   "RecipeId");

            migrationBuilder.Sql("ALTER TABLE [RecipeIngredients] DROP COLUMN [RecipeId];");
            migrationBuilder.Sql("ALTER TABLE [MealPlanRecipes]   DROP COLUMN [RecipeId];");
            migrationBuilder.Sql("ALTER TABLE [Recipes] DROP COLUMN [Id];");

            migrationBuilder.Sql("EXEC sp_rename N'[Recipes].[NewId]',                  N'Id',       N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[RecipeIngredients].[NewRecipeId]', N'RecipeId', N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[MealPlanRecipes].[NewRecipeId]',   N'RecipeId', N'COLUMN';");

            migrationBuilder.Sql("ALTER TABLE [RecipeIngredients] ALTER COLUMN [RecipeId] int NOT NULL;");
            migrationBuilder.Sql("ALTER TABLE [MealPlanRecipes]   ALTER COLUMN [RecipeId] int NOT NULL;");

            migrationBuilder.Sql(
                "ALTER TABLE [Recipes] ADD CONSTRAINT [PK_Recipes] PRIMARY KEY ([Id]);");
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeIngredients] ADD CONSTRAINT [PK_RecipeIngredients] " +
                "PRIMARY KEY ([RecipeId], [ProductId]);");
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] ADD CONSTRAINT [PK_MealPlanRecipes] " +
                "PRIMARY KEY ([MealPlanId], [RecipeId]);");
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeIngredients] ADD CONSTRAINT [FK_RecipeIngredients_Recipes_RecipeId] " +
                "FOREIGN KEY ([RecipeId]) REFERENCES [Recipes] ([Id]) ON DELETE CASCADE;");
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] ADD CONSTRAINT [FK_MealPlanRecipes_Recipes_RecipeId] " +
                "FOREIGN KEY ([RecipeId]) REFERENCES [Recipes] ([Id]) ON DELETE NO ACTION;");
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
