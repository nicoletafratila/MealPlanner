using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class ProductCategoryIdToGuid : Migration
    {
        // Switches ProductCategories.Id (and the Products.ProductCategoryId / ShopDisplaySequences.ProductCategoryId
        // FKs) from an int IDENTITY primary key to a uniqueidentifier, preserving existing rows and the child-row
        // linkage. EF's scaffolded AlterColumn cannot convert int -> uniqueidentifier in place, so this is
        // hand-written: add the new Guid columns, backfill the children from the old int mapping, then drop and
        // recreate the keys/indexes/FKs against the new columns. ShopDisplaySequences keeps its composite primary
        // key [ShopId, ProductCategoryId].
        //
        // The foreign keys, primary keys and the ProductCategoryId indexes are dropped by their *actual* names
        // looked up from the system catalogs rather than the EF-convention names, because a database created via
        // EnsureCreated (or by hand) may name those objects differently than the InitialCreate migration.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add the new Guid columns. ProductCategories.NewId gets a temporary NEWID() default so existing rows
            //    receive distinct values; the child NewProductCategoryId columns are filled below.
            migrationBuilder.Sql(
                "ALTER TABLE [ProductCategories] ADD [NewId] uniqueidentifier NOT NULL " +
                "CONSTRAINT [DF_ProductCategories_NewId] DEFAULT NEWID();");
            migrationBuilder.Sql(
                "ALTER TABLE [Products] ADD [NewProductCategoryId] uniqueidentifier NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShopDisplaySequences] ADD [NewProductCategoryId] uniqueidentifier NULL;");

            // 2. Map each child row to its category's freshly generated Guid.
            migrationBuilder.Sql(
                "UPDATE p SET p.[NewProductCategoryId] = pc.[NewId] " +
                "FROM [Products] p " +
                "INNER JOIN [ProductCategories] pc ON p.[ProductCategoryId] = pc.[Id];");
            migrationBuilder.Sql(
                "UPDATE sds SET sds.[NewProductCategoryId] = pc.[NewId] " +
                "FROM [ShopDisplaySequences] sds " +
                "INNER JOIN [ProductCategories] pc ON sds.[ProductCategoryId] = pc.[Id];");

            // 2b. Remove rows that reference a category that no longer exists (their NewProductCategoryId could not
            //     be mapped), so the columns can become NOT NULL and the FKs can be recreated. Products are
            //     referenced by RecipeIngredients and ShoppingListProducts, so those children are removed first.
            migrationBuilder.Sql(
                "DELETE FROM [RecipeIngredients] " +
                "WHERE [ProductId] IN (SELECT [Id] FROM [Products] WHERE [NewProductCategoryId] IS NULL);");
            migrationBuilder.Sql(
                "DELETE FROM [ShoppingListProducts] " +
                "WHERE [ProductId] IN (SELECT [Id] FROM [Products] WHERE [NewProductCategoryId] IS NULL);");
            migrationBuilder.Sql(
                "DELETE FROM [Products] WHERE [NewProductCategoryId] IS NULL;");
            migrationBuilder.Sql(
                "DELETE FROM [ShopDisplaySequences] WHERE [NewProductCategoryId] IS NULL;");

            // 3. Drop the FKs that point at ProductCategories, the ProductCategoryId indexes and the primary keys
            //    tied to the old int columns - all by their real names.
            DropForeignKeysReferencing(migrationBuilder, "Products", "ProductCategories");
            DropForeignKeysReferencing(migrationBuilder, "ShopDisplaySequences", "ProductCategories");
            DropIndexesOnColumn(migrationBuilder, "Products", "ProductCategoryId");
            DropIndexesOnColumn(migrationBuilder, "ShopDisplaySequences", "ProductCategoryId");
            DropPrimaryKey(migrationBuilder, "ShopDisplaySequences");
            DropPrimaryKey(migrationBuilder, "ProductCategories");

            // 4. Remove the old int columns and the temporary default.
            migrationBuilder.Sql("ALTER TABLE [Products] DROP COLUMN [ProductCategoryId];");
            migrationBuilder.Sql("ALTER TABLE [ShopDisplaySequences] DROP COLUMN [ProductCategoryId];");
            migrationBuilder.Sql("ALTER TABLE [ProductCategories] DROP CONSTRAINT [DF_ProductCategories_NewId];");
            migrationBuilder.Sql("ALTER TABLE [ProductCategories] DROP COLUMN [Id];");

            // 5. Rename the new columns to the original names.
            migrationBuilder.Sql("EXEC sp_rename N'[ProductCategories].[NewId]', N'Id', N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[Products].[NewProductCategoryId]', N'ProductCategoryId', N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[ShopDisplaySequences].[NewProductCategoryId]', N'ProductCategoryId', N'COLUMN';");

            // 6. Restore NOT NULL on the child FK columns now that they are fully populated.
            migrationBuilder.Sql(
                "ALTER TABLE [Products] ALTER COLUMN [ProductCategoryId] uniqueidentifier NOT NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShopDisplaySequences] ALTER COLUMN [ProductCategoryId] uniqueidentifier NOT NULL;");

            // 7. Recreate the primary keys, the indexes and the foreign keys against the new Guid columns.
            migrationBuilder.Sql(
                "ALTER TABLE [ProductCategories] ADD CONSTRAINT [PK_ProductCategories] PRIMARY KEY ([Id]);");
            migrationBuilder.Sql(
                "ALTER TABLE [ShopDisplaySequences] ADD CONSTRAINT [PK_ShopDisplaySequences] PRIMARY KEY ([ShopId], [ProductCategoryId]);");
            migrationBuilder.Sql(
                "CREATE INDEX [IX_Products_ProductCategoryId] ON [Products] ([ProductCategoryId]);");
            migrationBuilder.Sql(
                "CREATE INDEX [IX_ShopDisplaySequences_ProductCategoryId] ON [ShopDisplaySequences] ([ProductCategoryId]);");
            migrationBuilder.Sql(
                "ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_ProductCategories_ProductCategoryId] " +
                "FOREIGN KEY ([ProductCategoryId]) REFERENCES [ProductCategories] ([Id]) ON DELETE CASCADE;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShopDisplaySequences] ADD CONSTRAINT [FK_ShopDisplaySequences_ProductCategories_ProductCategoryId] " +
                "FOREIGN KEY ([ProductCategoryId]) REFERENCES [ProductCategories] ([Id]) ON DELETE CASCADE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverts to an int IDENTITY primary key. Original int ids cannot be recovered, so a fresh identity
            // sequence is assigned while preserving the child-row linkage.
            migrationBuilder.Sql(
                "ALTER TABLE [ProductCategories] ADD [NewId] int IDENTITY(1,1) NOT NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [Products] ADD [NewProductCategoryId] int NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShopDisplaySequences] ADD [NewProductCategoryId] int NULL;");

            migrationBuilder.Sql(
                "UPDATE p SET p.[NewProductCategoryId] = pc.[NewId] " +
                "FROM [Products] p " +
                "INNER JOIN [ProductCategories] pc ON p.[ProductCategoryId] = pc.[Id];");
            migrationBuilder.Sql(
                "UPDATE sds SET sds.[NewProductCategoryId] = pc.[NewId] " +
                "FROM [ShopDisplaySequences] sds " +
                "INNER JOIN [ProductCategories] pc ON sds.[ProductCategoryId] = pc.[Id];");

            DropForeignKeysReferencing(migrationBuilder, "Products", "ProductCategories");
            DropForeignKeysReferencing(migrationBuilder, "ShopDisplaySequences", "ProductCategories");
            DropIndexesOnColumn(migrationBuilder, "Products", "ProductCategoryId");
            DropIndexesOnColumn(migrationBuilder, "ShopDisplaySequences", "ProductCategoryId");
            DropPrimaryKey(migrationBuilder, "ShopDisplaySequences");
            DropPrimaryKey(migrationBuilder, "ProductCategories");

            migrationBuilder.Sql("ALTER TABLE [Products] DROP COLUMN [ProductCategoryId];");
            migrationBuilder.Sql("ALTER TABLE [ShopDisplaySequences] DROP COLUMN [ProductCategoryId];");
            migrationBuilder.Sql("ALTER TABLE [ProductCategories] DROP COLUMN [Id];");

            migrationBuilder.Sql("EXEC sp_rename N'[ProductCategories].[NewId]', N'Id', N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[Products].[NewProductCategoryId]', N'ProductCategoryId', N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[ShopDisplaySequences].[NewProductCategoryId]', N'ProductCategoryId', N'COLUMN';");

            migrationBuilder.Sql(
                "ALTER TABLE [Products] ALTER COLUMN [ProductCategoryId] int NOT NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShopDisplaySequences] ALTER COLUMN [ProductCategoryId] int NOT NULL;");

            migrationBuilder.Sql(
                "ALTER TABLE [ProductCategories] ADD CONSTRAINT [PK_ProductCategories] PRIMARY KEY ([Id]);");
            migrationBuilder.Sql(
                "ALTER TABLE [ShopDisplaySequences] ADD CONSTRAINT [PK_ShopDisplaySequences] PRIMARY KEY ([ShopId], [ProductCategoryId]);");
            migrationBuilder.Sql(
                "CREATE INDEX [IX_Products_ProductCategoryId] ON [Products] ([ProductCategoryId]);");
            migrationBuilder.Sql(
                "CREATE INDEX [IX_ShopDisplaySequences_ProductCategoryId] ON [ShopDisplaySequences] ([ProductCategoryId]);");
            migrationBuilder.Sql(
                "ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_ProductCategories_ProductCategoryId] " +
                "FOREIGN KEY ([ProductCategoryId]) REFERENCES [ProductCategories] ([Id]) ON DELETE CASCADE;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShopDisplaySequences] ADD CONSTRAINT [FK_ShopDisplaySequences_ProductCategories_ProductCategoryId] " +
                "FOREIGN KEY ([ProductCategoryId]) REFERENCES [ProductCategories] ([Id]) ON DELETE CASCADE;");
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
