using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class MealPlanRecipe_AddId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MealPlanRecipes",
                table: "MealPlanRecipes");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "MealPlanRecipes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MealPlanRecipes",
                table: "MealPlanRecipes",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_MealPlanRecipes_MealPlanId",
                table: "MealPlanRecipes",
                column: "MealPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MealPlanRecipes",
                table: "MealPlanRecipes");

            migrationBuilder.DropIndex(
                name: "IX_MealPlanRecipes_MealPlanId",
                table: "MealPlanRecipes");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "MealPlanRecipes");

            // The composite PK requires uniqueness on (MealPlanId, RecipeId).
            // If duplicate recipe entries were added while the single-Id PK was active,
            // remove the extras (keep one per pair) so the constraint can be restored.
            migrationBuilder.Sql(@"
                WITH Duplicates AS (
                    SELECT MealPlanId, RecipeId,
                           ROW_NUMBER() OVER (PARTITION BY MealPlanId, RecipeId ORDER BY (SELECT NULL)) AS rn
                    FROM MealPlanRecipes
                )
                DELETE FROM Duplicates WHERE rn > 1;");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MealPlanRecipes",
                table: "MealPlanRecipes",
                columns: new[] { "MealPlanId", "RecipeId" });
        }
    }
}
