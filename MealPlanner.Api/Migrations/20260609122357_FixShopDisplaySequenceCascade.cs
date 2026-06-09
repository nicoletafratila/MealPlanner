using Common.Data.DataContext;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixShopDisplaySequenceCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ForeignKeyExists<MealPlannerDbContext>("FK_ShopDisplaySequences_Shops_ShopId", "ShopDisplaySequences"))
                migrationBuilder.DropForeignKey(
                    name: "FK_ShopDisplaySequences_Shops_ShopId",
                    table: "ShopDisplaySequences");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopDisplaySequences_Shops_ShopId",
                table: "ShopDisplaySequences",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopDisplaySequences_Shops_ShopId",
                table: "ShopDisplaySequences");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopDisplaySequences_Shops_ShopId",
                table: "ShopDisplaySequences",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id");
        }
    }
}
