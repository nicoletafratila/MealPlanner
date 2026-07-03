using Common.Data.DataContext;

namespace RecipeBook.Api
{
    public static class SeedData
    {
        public static async Task EnsureSeedDataAsync(IServiceScope scope)
        {
            var context = scope.ServiceProvider.GetRequiredService<MealPlannerDbContext>();
            await context.EnsureSqlServerDatabaseCreatedAsync();
            context.EnsureSchemaCreated();

            await CategorySeedData.SeedProductCategoriesAsync(context);
            await CategorySeedData.SeedRecipeCategoriesAsync(context);
        }
    }
}
