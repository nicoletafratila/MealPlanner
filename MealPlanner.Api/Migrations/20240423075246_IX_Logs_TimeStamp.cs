using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class IX_Logs_TimeStamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex("IX_Logs_TimeStamp", "Logs", "TimeStamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("Logs", "IX_Logs_TimeStamp");
        }
    }
}
