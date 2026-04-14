using Common.Data.DataContext;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MealPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class Update_MealPlans_AddDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (!migrationBuilder.ColumnExists<MealPlannerDbContext>("CreatedAt", "MealPlans"))
                migrationBuilder.AddColumn<DateTime>(
                    name: "CreatedAt",
                    table: "MealPlans",
                    type: "datetime2",
                    nullable: true);

            if (!migrationBuilder.ColumnExists<MealPlannerDbContext>("UpdatedAt", "MealPlans"))
                migrationBuilder.AddColumn<DateTime>(
                    name: "UpdatedAt",
                    table: "MealPlans",
                    type: "datetime2",
                    nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("CreatedAt", "MealPlans"))
                migrationBuilder.DropColumn(
                    name: "CreatedAt",
                    table: "MealPlans");

            if (migrationBuilder.ColumnExists<MealPlannerDbContext>("UpdatedAt", "MealPlans"))
                migrationBuilder.DropColumn(
                    name: "UpdatedAt",
                    table: "MealPlans");
        }
    }
}