using CommonServiceLocator;
using Microsoft.AspNetCore.Components;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;

namespace Common.Data.DataContext
{
    public static class MigrationBuilderExtensions
    {
        [Inject]
        public static IConfiguration? Configuration { get; set; }

        public static bool TableExists<TDbContext>(this MigrationBuilder migrationBuilder, string tableName, string? schemaName = null) where TDbContext : DbContext
        {
            //var connectionString = ServiceLocator.Current.GetInstance<IConfiguration>()?.GetConnectionString("MealPlanner"); //ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString;

            //var connectionString = ConfigurationManager.ConnectionStrings["MealPlanner"].ConnectionString;
            //var dbContextOptions = new DbContextOptions<MealPlannerDbContext>();
            //using (var context = new DbContext(dbContextOptions))
            //{
            //    var parameters = new List<SqlParameter>()
            //    {
            //        new SqlParameter(nameof(tableName), tableName)
            //    };
            //    if (!string.IsNullOrWhiteSpace(schemaName))
            //    {
            //        parameters.Add(new SqlParameter(nameof(schemaName), schemaName));
            //    }

            //    var result = context.Database.SqlQueryRaw<int>("SELECT COUNT(OBJECT_ID(@schemaName + '.' + @tableName, 'U'))", parameters);
            //    return result.Any(x => x > 0);
            //}

            //var tableNameSQLStringBuilder = new StringBuilder();
            //tableNameSQLStringBuilder.Append($"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = {tableName} ");
            //if (schemaName != null)
            //{
            //    tableNameSQLStringBuilder.Append($" AND TABLE_SCHEMA = {schemaName}");
            //}
            //return migrationBuilder.Sql(tableNameSQLStringBuilder.ToString());

            //var connectionstring = ConfigurationManager.ConnectionStrings["MealPlanner"].ConnectionString;
            //var optionsBuilder = new DbContextOptionsBuilder<MealPlannerDbContext>();
            //optionsBuilder.UseSqlServer(connectionstring);
            //using (MealPlannerDbContext dbContext = new MealPlannerDbContext(optionsBuilder.Options))
            //{
            //    //...do stuff
            //}

            //TDbContext context = ServiceLocator.Current.GetInstance<TDbContext>();
            //if (!context.Database.CanConnect())
            //{
            //    return false;
            //}
            //var parameters = new List<SqlParameter>()
            //    {
            //        new SqlParameter(nameof(tableName), tableName)
            //    };
            //if (!string.IsNullOrWhiteSpace(schemaName))
            //{
            //    parameters.Add(new SqlParameter(nameof(schemaName), schemaName));
            //}

            //var result = context.Database.SqlQueryRaw<int>("SELECT COUNT(OBJECT_ID(@schemaName + '.' + @tableName, 'U'))", parameters);
            //return result.Any(x => x > 0);
        }

        public static bool IndexExists<TDbContext>(this MigrationBuilder migrationBuilder, string indexName, string tableName, string? schemaName = null) where TDbContext : DbContext
        {
            TDbContext context = ServiceLocator.Current.GetInstance<TDbContext>();
            if (!context.Database.CanConnect())
            {
                return false;
            }

            var parameters = new List<SqlParameter>()
            {
                new SqlParameter(nameof(tableName), tableName),
                new SqlParameter(nameof(indexName), indexName)
            };
            if (!string.IsNullOrWhiteSpace(schemaName))
            {
                parameters.Add(new SqlParameter(nameof(schemaName), schemaName));
            }

            var result = context.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM sys.indexes WHERE object_id = OBJECT_ID(@schemaName + '.' + @tableName) AND name = @indexName", parameters);
            return result.Any(x => x > 0);
        }
    }
}
