using Common.Data.DataContext;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class Products_BaseUnitId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (!migrationBuilder.ColumnExists<MealPlannerDbContext>("BaseUnitId", "Products"))
            {
                migrationBuilder.AddColumn<int>(
                name: "BaseUnitId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
            }

            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_Products_BaseUnitId", "Products"))
            {
                migrationBuilder.CreateIndex(
                name: "IX_Products_BaseUnitId",
                table: "Products",
                column: "BaseUnitId");
            }

            if (!migrationBuilder.ForeignKeyExists<MealPlannerDbContext>("FK_Products_Units_BaseUnitId", "Products"))
            {
                migrationBuilder.AddForeignKey(
                name: "FK_Products_Units_BaseUnitId",
                table: "Products",
                column: "BaseUnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Units_BaseUnitId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_BaseUnitId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BaseUnitId",
                table: "Products");
        }
    }
}
