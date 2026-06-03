using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class ShoppingListIdToGuid : Migration
    {
        // Switches ShoppingLists.Id (and the ShoppingListProducts.ShoppingListId FK) from an int IDENTITY
        // primary key to a uniqueidentifier, preserving existing rows and the child-row linkage. EF's
        // scaffolded AlterColumn cannot convert int -> uniqueidentifier in place, so this is hand-written:
        // add the new Guid columns, backfill the products from the old int mapping, drop the products that
        // reference a list that no longer exists, then drop and recreate the keys/FK against the new columns.
        //
        // Foreign keys and primary keys are dropped by their *actual* names looked up from the system
        // catalogs rather than the EF-convention names, because a database created via EnsureCreated (or by
        // hand) may name those objects differently (e.g. PK_ShoppingList) or omit the FK entirely.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add the new Guid columns. ShoppingLists.NewId gets a temporary NEWID() default so existing
            //    rows receive distinct values; ShoppingListProducts.NewShoppingListId is filled below.
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingLists] ADD [NewId] uniqueidentifier NOT NULL " +
                "CONSTRAINT [DF_ShoppingLists_NewId] DEFAULT NEWID();");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingListProducts] ADD [NewShoppingListId] uniqueidentifier NULL;");

            // 2. Map each product to its list's freshly generated Guid.
            migrationBuilder.Sql(
                "UPDATE slp SET slp.[NewShoppingListId] = sl.[NewId] " +
                "FROM [ShoppingListProducts] slp " +
                "INNER JOIN [ShoppingLists] sl ON slp.[ShoppingListId] = sl.[Id];");

            // 2b. Remove products that reference a list that no longer exists (their NewShoppingListId could
            //     not be mapped), so the column can become NOT NULL and the FK can be created.
            migrationBuilder.Sql(
                "DELETE FROM [ShoppingListProducts] WHERE [NewShoppingListId] IS NULL;");

            // 3. Drop the FKs that point at ShoppingLists and the primary keys tied to the old int columns -
            //    all by their real names.
            DropForeignKeysReferencing(migrationBuilder, "ShoppingListProducts", "ShoppingLists");
            DropPrimaryKey(migrationBuilder, "ShoppingListProducts");
            DropPrimaryKey(migrationBuilder, "ShoppingLists");

            // 4. Remove the old int columns and the temporary default.
            migrationBuilder.Sql("ALTER TABLE [ShoppingListProducts] DROP COLUMN [ShoppingListId];");
            migrationBuilder.Sql("ALTER TABLE [ShoppingLists] DROP CONSTRAINT [DF_ShoppingLists_NewId];");
            migrationBuilder.Sql("ALTER TABLE [ShoppingLists] DROP COLUMN [Id];");

            // 5. Rename the new columns to the original names.
            migrationBuilder.Sql("EXEC sp_rename N'[ShoppingLists].[NewId]', N'Id', N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[ShoppingListProducts].[NewShoppingListId]', N'ShoppingListId', N'COLUMN';");

            // 6. Restore NOT NULL on the child FK column now that it is fully populated.
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingListProducts] ALTER COLUMN [ShoppingListId] uniqueidentifier NOT NULL;");

            // 7. Recreate the primary keys and the foreign key against the new Guid columns.
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingLists] ADD CONSTRAINT [PK_ShoppingLists] PRIMARY KEY ([Id]);");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingListProducts] ADD CONSTRAINT [PK_ShoppingListProducts] PRIMARY KEY ([ShoppingListId], [ProductId]);");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingListProducts] ADD CONSTRAINT [FK_ShoppingListProducts_ShoppingLists_ShoppingListId] " +
                "FOREIGN KEY ([ShoppingListId]) REFERENCES [ShoppingLists] ([Id]) ON DELETE NO ACTION;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverts to an int IDENTITY primary key. Original int ids cannot be recovered, so a fresh
            // identity sequence is assigned while preserving the child-row linkage.
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingLists] ADD [NewId] int IDENTITY(1,1) NOT NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingListProducts] ADD [NewShoppingListId] int NULL;");

            migrationBuilder.Sql(
                "UPDATE slp SET slp.[NewShoppingListId] = sl.[NewId] " +
                "FROM [ShoppingListProducts] slp " +
                "INNER JOIN [ShoppingLists] sl ON slp.[ShoppingListId] = sl.[Id];");
            migrationBuilder.Sql(
                "DELETE FROM [ShoppingListProducts] WHERE [NewShoppingListId] IS NULL;");

            DropForeignKeysReferencing(migrationBuilder, "ShoppingListProducts", "ShoppingLists");
            DropPrimaryKey(migrationBuilder, "ShoppingListProducts");
            DropPrimaryKey(migrationBuilder, "ShoppingLists");

            migrationBuilder.Sql("ALTER TABLE [ShoppingListProducts] DROP COLUMN [ShoppingListId];");
            migrationBuilder.Sql("ALTER TABLE [ShoppingLists] DROP COLUMN [Id];");

            migrationBuilder.Sql("EXEC sp_rename N'[ShoppingLists].[NewId]', N'Id', N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[ShoppingListProducts].[NewShoppingListId]', N'ShoppingListId', N'COLUMN';");

            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingListProducts] ALTER COLUMN [ShoppingListId] int NOT NULL;");

            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingLists] ADD CONSTRAINT [PK_ShoppingLists] PRIMARY KEY ([Id]);");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingListProducts] ADD CONSTRAINT [PK_ShoppingListProducts] PRIMARY KEY ([ShoppingListId], [ProductId]);");
            migrationBuilder.Sql(
                "ALTER TABLE [ShoppingListProducts] ADD CONSTRAINT [FK_ShoppingListProducts_ShoppingLists_ShoppingListId] " +
                "FOREIGN KEY ([ShoppingListId]) REFERENCES [ShoppingLists] ([Id]) ON DELETE NO ACTION;");
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
    }
}
