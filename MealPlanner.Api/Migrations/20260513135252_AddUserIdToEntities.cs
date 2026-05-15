using Common.Data.DataContext;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (!migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "Shops"))
                migrationBuilder.AddColumn<string>(
                    name: "UserId",
                    table: "Shops",
                    type: "nvarchar(max)",
                    nullable: false,
                    defaultValue: "");

            if (!migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "ShoppingLists"))
                migrationBuilder.AddColumn<string>(
                    name: "UserId",
                    table: "ShoppingLists",
                    type: "nvarchar(max)",
                    nullable: false,
                    defaultValue: "");

            if (!migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "Recipes"))
                migrationBuilder.AddColumn<string>(
                    name: "UserId",
                    table: "Recipes",
                    type: "nvarchar(max)",
                    nullable: false,
                    defaultValue: "");

            if (!migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "RecipeCategories"))
                migrationBuilder.AddColumn<string>(
                    name: "UserId",
                    table: "RecipeCategories",
                    type: "nvarchar(max)",
                    nullable: true,
                    defaultValue: "");

            if (!migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "Products"))
                migrationBuilder.AddColumn<string>(
                    name: "UserId",
                    table: "Products",
                    type: "nvarchar(max)",
                    nullable: false,
                    defaultValue: "");

            if (!migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "ProductCategories"))
                migrationBuilder.AddColumn<string>(
                    name: "UserId",
                    table: "ProductCategories",
                    type: "nvarchar(max)",
                    nullable: true,
                    defaultValue: "");

            if (!migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "MealPlans"))
                migrationBuilder.AddColumn<string>(
                    name: "UserId",
                    table: "MealPlans",
                    type: "nvarchar(max)",
                    nullable: false,
                    defaultValue: "");

            migrationBuilder.Sql("""
                DECLARE @adminId NVARCHAR(450) = (SELECT [Id] FROM [AspNetUsers] WHERE [UserName] = 'admin');
                IF @adminId IS NOT NULL
                BEGIN
                    UPDATE [Shops]             SET [UserId] = @adminId WHERE [UserId] = '';
                    UPDATE [ShoppingLists]     SET [UserId] = @adminId WHERE [UserId] = '';
                    UPDATE [Recipes]           SET [UserId] = @adminId WHERE [UserId] = '';
                    UPDATE [Products]          SET [UserId] = @adminId WHERE [UserId] = '';
                    UPDATE [MealPlans]         SET [UserId] = @adminId WHERE [UserId] = '';
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "Shops"))
                migrationBuilder.DropColumn(
                    name: "UserId",
                    table: "Shops");

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "ShoppingLists"))
                migrationBuilder.DropColumn(
                    name: "UserId",
                    table: "ShoppingLists");

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "Recipes"))
                migrationBuilder.DropColumn(
                    name: "UserId",
                    table: "Recipes");

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "RecipeCategories"))
                migrationBuilder.DropColumn(
                    name: "UserId",
                    table: "RecipeCategories");

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "Products"))
                migrationBuilder.DropColumn(
                    name: "UserId",
                    table: "Products");

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "ProductCategories"))
                migrationBuilder.DropColumn(
                    name: "UserId",
                    table: "ProductCategories");

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "MealPlans"))
                migrationBuilder.DropColumn(
                    name: "UserId",
                    table: "MealPlans");
        }
    }
}
