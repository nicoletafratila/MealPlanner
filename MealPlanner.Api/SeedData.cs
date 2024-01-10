using Common.Data.DataContext;

namespace MealPlanner.Api
{
    public class SeedData
    {
        public static async Task EnsureSeedData(IServiceScope scope)
        {
            var context = scope.ServiceProvider.GetService<MealPlannerDbContext>();
            //if (!context!.Database.EnsureCreated())
            //    await context!.Database.MigrateAsync();
            await SeedProductCategories(scope);
        }

        private static async Task SeedProductCategories(IServiceScope scope)
        { }
    }
}
