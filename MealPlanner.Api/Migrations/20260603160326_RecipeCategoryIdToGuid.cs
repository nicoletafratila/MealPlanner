using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class RecipeCategoryIdToGuid : Migration
    {
        // Switches RecipeCategories.Id (and the Recipes.RecipeCategoryId FK) from an int IDENTITY primary key to a
        // uniqueidentifier, preserving existing rows and the child-row linkage. EF's scaffolded AlterColumn cannot
        // convert int -> uniqueidentifier in place, so this is hand-written: add the new Guid columns, backfill the
        // recipes from the old int mapping, then drop and recreate the keys/indexes/FK against the new columns.
        //
        // The foreign key, primary key and the RecipeCategoryId index are dropped by their *actual* names looked up
        // from the system catalogs rather than the EF-convention names, because a database created via EnsureCreated
        // (or by hand) may name those objects differently than the InitialCreate migration.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add the new Guid columns. RecipeCategories.NewId gets a temporary NEWID() default so existing rows
            //    receive distinct values; the child NewRecipeCategoryId column is filled below.
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeCategories] ADD [NewId] uniqueidentifier NOT NULL " +
                "CONSTRAINT [DF_RecipeCategories_NewId] DEFAULT NEWID();");
            migrationBuilder.Sql(
                "ALTER TABLE [Recipes] ADD [NewRecipeCategoryId] uniqueidentifier NULL;");

            // 2. Map each recipe to its category's freshly generated Guid.
            migrationBuilder.Sql(
                "UPDATE r SET r.[NewRecipeCategoryId] = rc.[NewId] " +
                "FROM [Recipes] r " +
                "INNER JOIN [RecipeCategories] rc ON r.[RecipeCategoryId] = rc.[Id];");

            // 2b. Remove recipes that reference a category that no longer exists (their NewRecipeCategoryId could not
            //     be mapped), so the column can become NOT NULL and the FK can be recreated. Recipes are referenced
            //     by RecipeIngredients and MealPlanRecipes, so those children are removed first.
            migrationBuilder.Sql(
                "DELETE FROM [RecipeIngredients] " +
                "WHERE [RecipeId] IN (SELECT [Id] FROM [Recipes] WHERE [NewRecipeCategoryId] IS NULL);");
            migrationBuilder.Sql(
                "DELETE FROM [MealPlanRecipes] " +
                "WHERE [RecipeId] IN (SELECT [Id] FROM [Recipes] WHERE [NewRecipeCategoryId] IS NULL);");
            migrationBuilder.Sql(
                "DELETE FROM [Recipes] WHERE [NewRecipeCategoryId] IS NULL;");

            // 3. Drop the FK that points at RecipeCategories, the RecipeCategoryId index and the primary key tied to
            //    the old int column - all by their real names.
            DropForeignKeysReferencing(migrationBuilder, "Recipes", "RecipeCategories");
            DropIndexesOnColumn(migrationBuilder, "Recipes", "RecipeCategoryId");
            DropPrimaryKey(migrationBuilder, "RecipeCategories");

            // 4. Remove the old int columns and the temporary default.
            migrationBuilder.Sql("ALTER TABLE [Recipes] DROP COLUMN [RecipeCategoryId];");
            migrationBuilder.Sql("ALTER TABLE [RecipeCategories] DROP CONSTRAINT [DF_RecipeCategories_NewId];");
            migrationBuilder.Sql("ALTER TABLE [RecipeCategories] DROP COLUMN [Id];");

            // 5. Rename the new columns to the original names.
            migrationBuilder.Sql("EXEC sp_rename N'[RecipeCategories].[NewId]', N'Id', N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[Recipes].[NewRecipeCategoryId]', N'RecipeCategoryId', N'COLUMN';");

            // 6. Restore NOT NULL on the child FK column now that it is fully populated.
            migrationBuilder.Sql(
                "ALTER TABLE [Recipes] ALTER COLUMN [RecipeCategoryId] uniqueidentifier NOT NULL;");

            // 7. Recreate the primary key, the index and the foreign key against the new Guid columns.
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeCategories] ADD CONSTRAINT [PK_RecipeCategories] PRIMARY KEY ([Id]);");
            migrationBuilder.Sql(
                "CREATE INDEX [IX_Recipes_RecipeCategoryId] ON [Recipes] ([RecipeCategoryId]);");
            migrationBuilder.Sql(
                "ALTER TABLE [Recipes] ADD CONSTRAINT [FK_Recipes_RecipeCategories_RecipeCategoryId] " +
                "FOREIGN KEY ([RecipeCategoryId]) REFERENCES [RecipeCategories] ([Id]) ON DELETE CASCADE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverts to an int IDENTITY primary key. Original int ids cannot be recovered, so a fresh identity
            // sequence is assigned while preserving the child-row linkage.
            migrationBuilder.Sql(
                "ALTER TABLE [RecipeCategories] ADD [NewId] int IDENTITY(1,1) NOT NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [Recipes] ADD [NewRecipeCategoryId] int NULL;");

            migrationBuilder.Sql(
                "UPDATE r SET r.[NewRecipeCategoryId] = rc.[NewId] " +
                "FROM [Recipes] r " +
                "INNER JOIN [RecipeCategories] rc ON r.[RecipeCategoryId] = rc.[Id];");

            DropForeignKeysReferencing(migrationBuilder, "Recipes", "RecipeCategories");
            DropIndexesOnColumn(migrationBuilder, "Recipes", "RecipeCategoryId");
            DropPrimaryKey(migrationBuilder, "RecipeCategories");

            migrationBuilder.Sql("ALTER TABLE [Recipes] DROP COLUMN [RecipeCategoryId];");
            migrationBuilder.Sql("ALTER TABLE [RecipeCategories] DROP COLUMN [Id];");

            migrationBuilder.Sql("EXEC sp_rename N'[RecipeCategories].[NewId]', N'Id', N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[Recipes].[NewRecipeCategoryId]', N'RecipeCategoryId', N'COLUMN';");

            migrationBuilder.Sql(
                "ALTER TABLE [Recipes] ALTER COLUMN [RecipeCategoryId] int NOT NULL;");

            migrationBuilder.Sql(
                "ALTER TABLE [RecipeCategories] ADD CONSTRAINT [PK_RecipeCategories] PRIMARY KEY ([Id]);");
            migrationBuilder.Sql(
                "CREATE INDEX [IX_Recipes_RecipeCategoryId] ON [Recipes] ([RecipeCategoryId]);");
            migrationBuilder.Sql(
                "ALTER TABLE [Recipes] ADD CONSTRAINT [FK_Recipes_RecipeCategories_RecipeCategoryId] " +
                "FOREIGN KEY ([RecipeCategoryId]) REFERENCES [RecipeCategories] ([Id]) ON DELETE CASCADE;");
        }

        // Drops the foreign key on <table> that references <referencedTable>, by its actual name.
        private static void DropForeignKeysReferencing(MigrationBuilder migrationBuilder, string table, string referencedTable)
        {
            migrationBuilder.Sql(
                "DECLARE @fk sysname; " +
                "SELECT @fk = [name] FROM sys.foreign_keys " +
                "WHERE parent_object_id = OBJECT_ID(N'[" + table + "]') " +
                "AND referenced_object_id = OBJECT_ID(N'[" + referencedTable + "]'); " +
                "IF @fk IS NOT NULL EXEC(N'ALTER TABLE [" + table + "] DROP CONSTRAINT [' + @fk + N']');");
        }

        // Drops the primary key of <table>, by its actual name.
        private static void DropPrimaryKey(MigrationBuilder migrationBuilder, string table)
        {
            migrationBuilder.Sql(
                "DECLARE @pk sysname; " +
                "SELECT @pk = [name] FROM sys.key_constraints " +
                "WHERE parent_object_id = OBJECT_ID(N'[" + table + "]') AND [type] = 'PK'; " +
                "IF @pk IS NOT NULL EXEC(N'ALTER TABLE [" + table + "] DROP CONSTRAINT [' + @pk + N']');");
        }

        // Drops the non-key index on <table> that includes <column>, by its actual name.
        private static void DropIndexesOnColumn(MigrationBuilder migrationBuilder, string table, string column)
        {
            migrationBuilder.Sql(
                "DECLARE @ix sysname; " +
                "SELECT @ix = i.[name] FROM sys.indexes i " +
                "INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id " +
                "INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id " +
                "WHERE i.object_id = OBJECT_ID(N'[" + table + "]') AND c.[name] = N'" + column + "' " +
                "AND i.is_primary_key = 0 AND i.is_unique_constraint = 0; " +
                "IF @ix IS NOT NULL EXEC(N'DROP INDEX [' + @ix + N'] ON [" + table + "]');");
        }
    }
}
