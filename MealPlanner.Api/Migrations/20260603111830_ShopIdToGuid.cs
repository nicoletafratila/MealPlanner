using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class ShopIdToGuid : Migration
    {
        // Switches Shops.Id (and the ShopDisplaySequences.ShopId / ShoppingLists.ShopId FKs) from an
        // int IDENTITY primary key to a uniqueidentifier, preserving existing rows and the child-row
        // linkage. EF's scaffolded AlterColumn cannot convert int -> uniqueidentifier in place, so this
        // is hand-written: add the new Guid columns, backfill the children from the old int mapping,
        // then drop and recreate the keys/index/FKs against the new columns.
        //
        // The foreign keys, primary keys and the ShopId index are dropped by their *actual* names looked
        // up from the system catalogs rather than the EF-convention names, because a database created via
        // EnsureCreated (or by hand) may name those objects differently than the InitialCreate migration.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add the new Guid columns. Shops.NewId gets a temporary NEWID() default so existing rows
            //    receive distinct values; the child NewShopId columns are filled below.
            migrationBuilder.Sql(
                "ALTER TABLE [Shops] ADD [NewId] uniqueidentifier NOT NULL " +
                "CONSTRAINT [DF_Shops_NewId] DEFAULT NEWID();");
            migrationBuilder.Sql(
                "ALTER TABLE [ShopDisplaySequences] ADD [NewShopId] uniqueidentifier NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingLists] ADD [NewShopId] uniqueidentifier NULL;");

            // 2. Map each child row to its shop's freshly generated Guid.
            migrationBuilder.Sql(
                "UPDATE sds SET sds.[NewShopId] = s.[NewId] " +
                "FROM [ShopDisplaySequences] sds " +
                "INNER JOIN [Shops] s ON sds.[ShopId] = s.[Id];");
            migrationBuilder.Sql(
                "UPDATE sl SET sl.[NewShopId] = s.[NewId] " +
                "FROM [ShoppingLists] sl " +
                "INNER JOIN [Shops] s ON sl.[ShopId] = s.[Id];");

            // 2b. Remove rows that reference a shop that no longer exists (their NewShopId could not be
            //     mapped), so the column can become NOT NULL and the FK can be recreated. ShoppingList
            //     products are deleted first to satisfy their parent FK.
            migrationBuilder.Sql(
                "DELETE FROM [ShoppingListProducts] " +
                "WHERE [ShoppingListId] IN (SELECT [Id] FROM [ShoppingLists] WHERE [NewShopId] IS NULL);");
            migrationBuilder.Sql(
                "DELETE FROM [ShoppingLists] WHERE [NewShopId] IS NULL;");
            migrationBuilder.Sql(
                "DELETE FROM [ShopDisplaySequences] WHERE [NewShopId] IS NULL;");

            // 3. Drop the FKs that point at Shops, the ShopId index and the primary keys tied to the old
            //    int columns - all by their real names.
            DropForeignKeysReferencingShops(migrationBuilder, "ShopDisplaySequences");
            DropForeignKeysReferencingShops(migrationBuilder, "ShoppingLists");
            DropIndexesOnColumn(migrationBuilder, "ShoppingLists", "ShopId");
            DropPrimaryKey(migrationBuilder, "ShopDisplaySequences");
            DropPrimaryKey(migrationBuilder, "Shops");

            // 4. Remove the old int columns and the temporary default.
            migrationBuilder.Sql("ALTER TABLE [ShopDisplaySequences] DROP COLUMN [ShopId];");
            migrationBuilder.Sql("ALTER TABLE [ShoppingLists] DROP COLUMN [ShopId];");
            migrationBuilder.Sql("ALTER TABLE [Shops] DROP CONSTRAINT [DF_Shops_NewId];");
            migrationBuilder.Sql("ALTER TABLE [Shops] DROP COLUMN [Id];");

            // 5. Rename the new columns to the original names.
            migrationBuilder.Sql("EXEC sp_rename N'[Shops].[NewId]', N'Id', N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[ShopDisplaySequences].[NewShopId]', N'ShopId', N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[ShoppingLists].[NewShopId]', N'ShopId', N'COLUMN';");

            // 6. Restore NOT NULL on the child FK columns now that they are fully populated.
            migrationBuilder.Sql(
                "ALTER TABLE [ShopDisplaySequences] ALTER COLUMN [ShopId] uniqueidentifier NOT NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingLists] ALTER COLUMN [ShopId] uniqueidentifier NOT NULL;");

            // 7. Recreate the primary keys, the index and the foreign keys against the new Guid columns.
            migrationBuilder.Sql(
                "ALTER TABLE [Shops] ADD CONSTRAINT [PK_Shops] PRIMARY KEY ([Id]);");
            migrationBuilder.Sql(
                "ALTER TABLE [ShopDisplaySequences] ADD CONSTRAINT [PK_ShopDisplaySequences] PRIMARY KEY ([ShopId], [ProductCategoryId]);");
            migrationBuilder.Sql(
                "CREATE INDEX [IX_ShoppingLists_ShopId] ON [ShoppingLists] ([ShopId]);");
            migrationBuilder.Sql(
                "ALTER TABLE [ShopDisplaySequences] ADD CONSTRAINT [FK_ShopDisplaySequences_Shops_ShopId] " +
                "FOREIGN KEY ([ShopId]) REFERENCES [Shops] ([Id]) ON DELETE NO ACTION;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingLists] ADD CONSTRAINT [FK_ShoppingLists_Shops_ShopId] " +
                "FOREIGN KEY ([ShopId]) REFERENCES [Shops] ([Id]) ON DELETE NO ACTION;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverts to an int IDENTITY primary key. Original int ids cannot be recovered, so a fresh
            // identity sequence is assigned while preserving the child-row linkage.
            migrationBuilder.Sql(
                "ALTER TABLE [Shops] ADD [NewId] int IDENTITY(1,1) NOT NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShopDisplaySequences] ADD [NewShopId] int NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingLists] ADD [NewShopId] int NULL;");

            migrationBuilder.Sql(
                "UPDATE sds SET sds.[NewShopId] = s.[NewId] " +
                "FROM [ShopDisplaySequences] sds " +
                "INNER JOIN [Shops] s ON sds.[ShopId] = s.[Id];");
            migrationBuilder.Sql(
                "UPDATE sl SET sl.[NewShopId] = s.[NewId] " +
                "FROM [ShoppingLists] sl " +
                "INNER JOIN [Shops] s ON sl.[ShopId] = s.[Id];");

            DropForeignKeysReferencingShops(migrationBuilder, "ShopDisplaySequences");
            DropForeignKeysReferencingShops(migrationBuilder, "ShoppingLists");
            DropIndexesOnColumn(migrationBuilder, "ShoppingLists", "ShopId");
            DropPrimaryKey(migrationBuilder, "ShopDisplaySequences");
            DropPrimaryKey(migrationBuilder, "Shops");

            migrationBuilder.Sql("ALTER TABLE [ShopDisplaySequences] DROP COLUMN [ShopId];");
            migrationBuilder.Sql("ALTER TABLE [ShoppingLists] DROP COLUMN [ShopId];");
            migrationBuilder.Sql("ALTER TABLE [Shops] DROP COLUMN [Id];");

            migrationBuilder.Sql("EXEC sp_rename N'[Shops].[NewId]', N'Id', N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[ShopDisplaySequences].[NewShopId]', N'ShopId', N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[ShoppingLists].[NewShopId]', N'ShopId', N'COLUMN';");

            migrationBuilder.Sql(
                "ALTER TABLE [ShopDisplaySequences] ALTER COLUMN [ShopId] int NOT NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingLists] ALTER COLUMN [ShopId] int NOT NULL;");

            migrationBuilder.Sql(
                "ALTER TABLE [Shops] ADD CONSTRAINT [PK_Shops] PRIMARY KEY ([Id]);");
            migrationBuilder.Sql(
                "ALTER TABLE [ShopDisplaySequences] ADD CONSTRAINT [PK_ShopDisplaySequences] PRIMARY KEY ([ShopId], [ProductCategoryId]);");
            migrationBuilder.Sql(
                "CREATE INDEX [IX_ShoppingLists_ShopId] ON [ShoppingLists] ([ShopId]);");
            migrationBuilder.Sql(
                "ALTER TABLE [ShopDisplaySequences] ADD CONSTRAINT [FK_ShopDisplaySequences_Shops_ShopId] " +
                "FOREIGN KEY ([ShopId]) REFERENCES [Shops] ([Id]) ON DELETE NO ACTION;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingLists] ADD CONSTRAINT [FK_ShoppingLists_Shops_ShopId] " +
                "FOREIGN KEY ([ShopId]) REFERENCES [Shops] ([Id]) ON DELETE NO ACTION;");
        }

        // Drops the foreign key on <table> that references the Shops table, by its actual name.
        private static void DropForeignKeysReferencingShops(MigrationBuilder migrationBuilder, string table)
        {
            migrationBuilder.Sql(
                "DECLARE @fk sysname; " +
                "SELECT @fk = [name] FROM sys.foreign_keys " +
                "WHERE parent_object_id = OBJECT_ID(N'[" + table + "]') " +
                "AND referenced_object_id = OBJECT_ID(N'[Shops]'); " +
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
