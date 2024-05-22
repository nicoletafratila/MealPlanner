using Common.Data.DataContext;
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
            if (!migrationBuilder.IndexExists<MealPlannerDbContext>("IX_Logs_TimeStamp", ""))
            {
                migrationBuilder.CreateIndex("IX_Logs_TimeStamp", "Logs", "TimeStamp");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("Logs", "IX_Logs_TimeStamp");
        }
    }
}
