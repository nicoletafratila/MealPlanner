using Common.Data.DataContext;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class Recipe_Source : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (!migrationBuilder.ColumnExists<MealPlannerDbContext>("Source", "Recipes"))
            {
                migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Recipes",
                type: "nvarchar(max)",
                nullable: true);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "Recipes");
        }
    }
}
