using Common.Data.DataContext;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "Shops"))
                migrationBuilder.AlterColumn<string>(
                    name: "UserId",
                    table: "Shops",
                    type: "nvarchar(450)",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "nvarchar(max)",
                    oldNullable: true);

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_Shops_UserId", "Shops"))
                migrationBuilder.CreateIndex(
                    name: "IX_Shops_UserId",
                    table: "Shops",
                    column: "UserId");

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "ShoppingLists"))
                migrationBuilder.AlterColumn<string>(
                    name: "UserId",
                    table: "ShoppingLists",
                    type: "nvarchar(450)",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "nvarchar(max)",
                    oldNullable: true);

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_ShoppingLists_UserId", "ShoppingLists"))
                migrationBuilder.CreateIndex(
                    name: "IX_ShoppingLists_UserId",
                    table: "ShoppingLists",
                    column: "UserId");

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "Recipes"))
                migrationBuilder.AlterColumn<string>(
                    name: "UserId",
                    table: "Recipes",
                    type: "nvarchar(450)",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "nvarchar(max)",
                    oldNullable: true);

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_Recipes_UserId", "Recipes"))
                migrationBuilder.CreateIndex(
                    name: "IX_Recipes_UserId",
                    table: "Recipes",
                    column: "UserId");

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "RecipeCategories"))
                migrationBuilder.AlterColumn<string>(
                    name: "UserId",
                    table: "RecipeCategories",
                    type: "nvarchar(450)",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "nvarchar(max)",
                    oldNullable: true);

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_RecipeCategories_UserId", "RecipeCategories"))
                migrationBuilder.CreateIndex(
                    name: "IX_RecipeCategories_UserId",
                    table: "RecipeCategories",
                    column: "UserId");

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "Products"))
                migrationBuilder.AlterColumn<string>(
                    name: "UserId",
                    table: "Products",
                    type: "nvarchar(450)",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "nvarchar(max)",
                    oldNullable: true);

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_Products_UserId", "Products"))
                migrationBuilder.CreateIndex(
                    name: "IX_Products_UserId",
                    table: "Products",
                    column: "UserId");

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "ProductCategories"))
                migrationBuilder.AlterColumn<string>(
                    name: "UserId",
                    table: "ProductCategories",
                    type: "nvarchar(450)",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "nvarchar(max)",
                    oldNullable: true);

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_ProductCategories_UserId", "ProductCategories"))
                migrationBuilder.CreateIndex(
                    name: "IX_ProductCategories_UserId",
                    table: "ProductCategories",
                    column: "UserId");

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "MealPlans"))
                migrationBuilder.AlterColumn<string>(
                    name: "UserId",
                    table: "MealPlans",
                    type: "nvarchar(450)",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "nvarchar(max)",
                    oldNullable: true);

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_MealPlans_UserId", "MealPlans"))
                migrationBuilder.CreateIndex(
                    name: "IX_MealPlans_UserId",
                    table: "MealPlans",
                    column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.IndexExists<MealPlannerDbContext>("IX_Shops_UserId", "Shops"))
                migrationBuilder.DropIndex(
                    name: "IX_Shops_UserId",
                    table: "Shops");

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "Shops"))
                migrationBuilder.AlterColumn<string>(
                    name: "UserId",
                    table: "Shops",
                    type: "nvarchar(max)",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "nvarchar(450)",
                    oldNullable: true);

            if (migrationBuilder.IndexExists<MealPlannerDbContext>("IX_ShoppingLists_UserId", "ShoppingLists"))
                migrationBuilder.DropIndex(
                    name: "IX_ShoppingLists_UserId",
                    table: "ShoppingLists");

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "ShoppingLists"))
                migrationBuilder.AlterColumn<string>(
                    name: "UserId",
                    table: "ShoppingLists",
                    type: "nvarchar(max)",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "nvarchar(450)",
                    oldNullable: true);

            if (migrationBuilder.IndexExists<MealPlannerDbContext>("IX_Recipes_UserId", "Recipes"))
                migrationBuilder.DropIndex(
                    name: "IX_Recipes_UserId",
                    table: "Recipes");

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "Recipes"))
                migrationBuilder.AlterColumn<string>(
                    name: "UserId",
                    table: "Recipes",
                    type: "nvarchar(max)",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "nvarchar(450)",
                    oldNullable: true);

            if (migrationBuilder.IndexExists<MealPlannerDbContext>("IX_RecipeCategories_UserId", "RecipeCategories"))
                migrationBuilder.DropIndex(
                    name: "IX_RecipeCategories_UserId",
                    table: "RecipeCategories");

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "RecipeCategories"))
                migrationBuilder.AlterColumn<string>(
                    name: "UserId",
                    table: "RecipeCategories",
                    type: "nvarchar(max)",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "nvarchar(450)",
                    oldNullable: true);

            if (migrationBuilder.IndexExists<MealPlannerDbContext>("IX_Products_UserId", "Products"))
                migrationBuilder.DropIndex(
                    name: "IX_Products_UserId",
                    table: "Products");

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "Products"))
                migrationBuilder.AlterColumn<string>(
                    name: "UserId",
                    table: "Products",
                    type: "nvarchar(max)",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "nvarchar(450)",
                    oldNullable: true);

            if (migrationBuilder.IndexExists<MealPlannerDbContext>("IX_ProductCategories_UserId", "ProductCategories"))
                migrationBuilder.DropIndex(
                    name: "IX_ProductCategories_UserId",
                    table: "ProductCategories");

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "ProductCategories"))
                migrationBuilder.AlterColumn<string>(
                    name: "UserId",
                    table: "ProductCategories",
                    type: "nvarchar(max)",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "nvarchar(450)",
                    oldNullable: true);

            if (migrationBuilder.IndexExists<MealPlannerDbContext>("IX_MealPlans_UserId", "MealPlans"))
                migrationBuilder.DropIndex(
                    name: "IX_MealPlans_UserId",
                    table: "MealPlans");

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UserId", "MealPlans"))
                migrationBuilder.AlterColumn<string>(
                    name: "UserId",
                    table: "MealPlans",
                    type: "nvarchar(max)",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "nvarchar(450)",
                    oldNullable: true);
        }
    }
}
