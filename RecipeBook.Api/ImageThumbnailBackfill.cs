using Common.Data.DataContext;
using Microsoft.EntityFrameworkCore;
using RecipeBook.Api.Services;

namespace RecipeBook.Api
{
    /// <summary>
    /// One-time backfill for rows saved before the <c>ImageThumbnail</c> column existed.
    /// Idempotent: rows that already have a thumbnail are excluded from the query, so
    /// re-running this on every startup is a no-op once every row has been backfilled.
    /// </summary>
    public static class ImageThumbnailBackfill
    {
        private const int BatchSize = 50;

        public static async Task EnsureBackfilledAsync(MealPlannerDbContext context)
        {
            await BackfillRecipesAsync(context);
            await BackfillProductsAsync(context);
        }

        private static async Task BackfillRecipesAsync(MealPlannerDbContext context)
        {
            while (true)
            {
                var batch = await context.Recipes
                    .AsTracking()
                    .Where(x => x.ImageThumbnail == null && x.ImageContent != null)
                    .Take(BatchSize)
                    .ToListAsync();

                if (batch.Count == 0)
                    return;

                foreach (var recipe in batch)
                    recipe.ImageThumbnail = ImageThumbnailGenerator.CreateThumbnail(recipe.ImageContent);

                await context.SaveChangesAsync();
            }
        }

        private static async Task BackfillProductsAsync(MealPlannerDbContext context)
        {
            while (true)
            {
                var batch = await context.Products
                    .AsTracking()
                    .Where(x => x.ImageThumbnail == null && x.ImageContent != null)
                    .Take(BatchSize)
                    .ToListAsync();

                if (batch.Count == 0)
                    return;

                foreach (var product in batch)
                    product.ImageThumbnail = ImageThumbnailGenerator.CreateThumbnail(product.ImageContent);

                await context.SaveChangesAsync();
            }
        }
    }
}
