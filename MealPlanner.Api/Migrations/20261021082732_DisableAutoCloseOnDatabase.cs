using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Api.Migrations
{
    /// <inheritdoc />
    public partial class DisableAutoCloseOnDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DECLARE @dbName sysname = DB_NAME(); EXEC('ALTER DATABASE [' + @dbName + '] SET AUTO_CLOSE OFF');",
                suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DECLARE @dbName sysname = DB_NAME(); EXEC('ALTER DATABASE [' + @dbName + '] SET AUTO_CLOSE ON');",
                suppressTransaction: true);
        }
    }
}
