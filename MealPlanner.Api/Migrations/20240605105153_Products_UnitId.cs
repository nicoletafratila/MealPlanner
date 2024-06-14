using Common.Data.DataContext;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class Products_UnitId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (!migrationBuilder.ColumnExists<MealPlannerDbContext>("UnitId", "RecipeIngredients"))
            {
                migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "RecipeIngredients",
                type: "int",
                nullable: false,
                defaultValue: 0);
            }

            if (!migrationBuilder.ColumnExists<MealPlannerDbContext>("UnitId", "ShoppingListProducts"))
            {
                migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "ShoppingListProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);
            }

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("Quantity", "RecipeIngredients"))
            {
                migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "RecipeIngredients",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double(18,2)",
                oldPrecision: 18,
                oldScale: 2);
            }

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("Quantity", "ShoppingListProducts"))
            {
                migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "ShoppingListProducts",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double(18,2)",
                oldPrecision: 18,
                oldScale: 2);
            }

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_RecipeIngredients_UnitId", "RecipeIngredients"))
            {
                migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_UnitId",
                table: "RecipeIngredients",
                column: "UnitId");
            }

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_ShoppingListProducts_UnitId", "ShoppingListProducts"))
            {
                migrationBuilder.CreateIndex(
                name: "IX_ShoppingListProducts_UnitId",
                table: "ShoppingListProducts",
                column: "UnitId");
            }

            if (!migrationBuilder.ForeignKeyExists<MealPlannerDbContext>("FK_RecipeIngredients_Units_UnitId", "RecipeIngredients"))
            {
                migrationBuilder.AddForeignKey(
                name: "FK_RecipeIngredients_Units_UnitId",
                table: "RecipeIngredients",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            }

            if (!migrationBuilder.ForeignKeyExists<MealPlannerDbContext>("FK_ShoppingListProducts_Units_UnitId", "ShoppingListProducts"))
            {
                migrationBuilder.AddForeignKey(
                name: "FK_ShoppingListProducts_Units_UnitId",
                table: "ShoppingListProducts",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Quantity",
                table: "ShoppingListProducts",
                type: "double(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<double>(
                name: "Quantity",
                table: "RecipeIngredients",
                type: "double(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeIngredients_Units_UnitId",
                table: "RecipeIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingListProducts_Units_UnitId",
                table: "ShoppingListProducts");

            migrationBuilder.DropIndex(
               name: "IX_RecipeIngredients_UnitId",
               table: "RecipeIngredients");

            migrationBuilder.DropIndex(
               name: "IX_ShoppingListProducts_UnitId",
               table: "ShoppingListProducts");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "ShoppingListProducts");
        }
    }
}
