using System;
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
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeIngredients_Units_UnitId",
                table: "RecipeIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingListProducts_Units_UnitId",
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
