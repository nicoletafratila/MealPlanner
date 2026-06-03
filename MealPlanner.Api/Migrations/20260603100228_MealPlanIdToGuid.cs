using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class MealPlanIdToGuid : Migration
    {
        // Switches MealPlans.Id (and the MealPlanRecipes.MealPlanId FK) from an int IDENTITY
        // primary key to a uniqueidentifier, preserving existing rows and the junction-table
        // linkage. EF's scaffolded AlterColumn cannot convert int -> uniqueidentifier in place,
        // so this is hand-written: add the new Guid columns, backfill MealPlanRecipes from the
        // old int mapping, then drop and recreate the keys/FK against the new columns.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add the new Guid columns. MealPlans.NewId gets a temporary NEWID() default so
            //    existing rows receive distinct values; MealPlanRecipes.NewMealPlanId is filled below.
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlans] ADD [NewId] uniqueidentifier NOT NULL " +
                "CONSTRAINT [DF_MealPlans_NewId] DEFAULT NEWID();");
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] ADD [NewMealPlanId] uniqueidentifier NULL;");

            // 2. Map each junction row to its meal plan's freshly generated Guid.
            migrationBuilder.Sql(
                "UPDATE mpr SET mpr.[NewMealPlanId] = mp.[NewId] " +
                "FROM [MealPlanRecipes] mpr " +
                "INNER JOIN [MealPlans] mp ON mpr.[MealPlanId] = mp.[Id];");

            // 3. Drop the FK and both primary keys that depend on the old int columns.
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] DROP CONSTRAINT [FK_MealPlanRecipes_MealPlans_MealPlanId];");
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] DROP CONSTRAINT [PK_MealPlanRecipes];");
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlans] DROP CONSTRAINT [PK_MealPlans];");

            // 4. Remove the old int columns and the temporary default.
            migrationBuilder.Sql("ALTER TABLE [MealPlanRecipes] DROP COLUMN [MealPlanId];");
            migrationBuilder.Sql("ALTER TABLE [MealPlans] DROP CONSTRAINT [DF_MealPlans_NewId];");
            migrationBuilder.Sql("ALTER TABLE [MealPlans] DROP COLUMN [Id];");

            // 5. Rename the new columns to the original names.
            migrationBuilder.Sql("EXEC sp_rename N'[MealPlans].[NewId]', N'Id', N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[MealPlanRecipes].[NewMealPlanId]', N'MealPlanId', N'COLUMN';");

            // 6. Restore NOT NULL on the junction FK column now that it is fully populated.
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] ALTER COLUMN [MealPlanId] uniqueidentifier NOT NULL;");

            // 7. Recreate the primary keys and the foreign key against the new Guid columns.
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlans] ADD CONSTRAINT [PK_MealPlans] PRIMARY KEY ([Id]);");
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] ADD CONSTRAINT [PK_MealPlanRecipes] PRIMARY KEY ([MealPlanId], [RecipeId]);");
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] ADD CONSTRAINT [FK_MealPlanRecipes_MealPlans_MealPlanId] " +
                "FOREIGN KEY ([MealPlanId]) REFERENCES [MealPlans] ([Id]) ON DELETE NO ACTION;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverts to an int IDENTITY primary key. Original int ids cannot be recovered, so a
            // fresh identity sequence is assigned while preserving the MealPlanRecipes linkage.
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlans] ADD [NewId] int IDENTITY(1,1) NOT NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] ADD [NewMealPlanId] int NULL;");

            migrationBuilder.Sql(
                "UPDATE mpr SET mpr.[NewMealPlanId] = mp.[NewId] " +
                "FROM [MealPlanRecipes] mpr " +
                "INNER JOIN [MealPlans] mp ON mpr.[MealPlanId] = mp.[Id];");

            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] DROP CONSTRAINT [FK_MealPlanRecipes_MealPlans_MealPlanId];");
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] DROP CONSTRAINT [PK_MealPlanRecipes];");
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlans] DROP CONSTRAINT [PK_MealPlans];");

            migrationBuilder.Sql("ALTER TABLE [MealPlanRecipes] DROP COLUMN [MealPlanId];");
            migrationBuilder.Sql("ALTER TABLE [MealPlans] DROP COLUMN [Id];");

            migrationBuilder.Sql("EXEC sp_rename N'[MealPlans].[NewId]', N'Id', N'COLUMN';");
            migrationBuilder.Sql("EXEC sp_rename N'[MealPlanRecipes].[NewMealPlanId]', N'MealPlanId', N'COLUMN';");

            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] ALTER COLUMN [MealPlanId] int NOT NULL;");

            migrationBuilder.Sql(
                "ALTER TABLE [MealPlans] ADD CONSTRAINT [PK_MealPlans] PRIMARY KEY ([Id]);");
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] ADD CONSTRAINT [PK_MealPlanRecipes] PRIMARY KEY ([MealPlanId], [RecipeId]);");
            migrationBuilder.Sql(
                "ALTER TABLE [MealPlanRecipes] ADD CONSTRAINT [FK_MealPlanRecipes_MealPlans_MealPlanId] " +
                "FOREIGN KEY ([MealPlanId]) REFERENCES [MealPlans] ([Id]) ON DELETE NO ACTION;");
        }
    }
}
