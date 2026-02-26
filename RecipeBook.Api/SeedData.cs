using Common.Data.DataContext;
using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace RecipeBook.Api
{
    public static class SeedData
    {
        public static async Task EnsureSeedDataAsync(IServiceScope scope)
        {
            var context = scope.ServiceProvider.GetRequiredService<MealPlannerDbContext>();

            await context.Database.EnsureCreatedAsync();

            await SeedProductCategoriesAsync(context);
            await SeedRecipeCategoriesAsync(context);
        }

        private static async Task SeedProductCategoriesAsync(MealPlannerDbContext context)
        {
            if (await context.ProductCategories.AnyAsync())
                return;

            var categories = new[]
            {
                new ProductCategory { Name = "Lactate" },
                new ProductCategory { Name = "Mezeluri" },
                new ProductCategory { Name = "Legume" },
                new ProductCategory { Name = "Fructe" },
                new ProductCategory { Name = "Condimente" },
                new ProductCategory { Name = "Carne" },
                new ProductCategory { Name = "Conserve" },
                new ProductCategory { Name = "Ulei/Otet" },
                new ProductCategory { Name = "Fructe uscate" },
                new ProductCategory { Name = "Paste" },
                new ProductCategory { Name = "Branzeturi" },
                new ProductCategory { Name = "Fainoase" },
                new ProductCategory { Name = "Sosuri" },
                new ProductCategory { Name = "Congelate" },
                new ProductCategory { Name = "Apa" },
                new ProductCategory { Name = "Cofetarie/Patiserie" },
                new ProductCategory { Name = "Brutarie" },
                new ProductCategory { Name = "Peste" },
                new ProductCategory { Name = "Produse bucatarie" },
                new ProductCategory { Name = "Cereale" },
                new ProductCategory { Name = "Rechizite" },
                new ProductCategory { Name = "Jucarii" },
                new ProductCategory { Name = "Detergenti" },
                new ProductCategory { Name = "Haine" },
                new ProductCategory { Name = "Alcool" },
                new ProductCategory { Name = "Produse igena" },
                new ProductCategory { Name = "Sucuri" },
                new ProductCategory { Name = "Ceai/Cafea" },
                new ProductCategory { Name = "Hartie/Servetele" },
                new ProductCategory { Name = "Snaks" },
                new ProductCategory { Name = "Dulciuri" }
            };

            await context.ProductCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        private static async Task SeedRecipeCategoriesAsync(MealPlannerDbContext context)
        {
            if (await context.RecipeCategories.AnyAsync())
                return;

            var recipeCategories = new[]
            {
                new RecipeCategory { Name = "Aperitive/Gustari",   DisplaySequence = 1 },
                new RecipeCategory { Name = "Supe/Ciorbe",         DisplaySequence = 2 },
                new RecipeCategory { Name = "Fel principal",       DisplaySequence = 3 },
                new RecipeCategory { Name = "Garnituri",           DisplaySequence = 4 },
                new RecipeCategory { Name = "Paste",               DisplaySequence = 5 },
                new RecipeCategory { Name = "Salate",              DisplaySequence = 6 },
                new RecipeCategory { Name = "Desert",              DisplaySequence = 7 }
            };

            await context.RecipeCategories.AddRangeAsync(recipeCategories);
            await context.SaveChangesAsync();
        }
    }
}