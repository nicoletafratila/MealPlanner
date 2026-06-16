using Common.Data.DataContext;
using Microsoft.EntityFrameworkCore;
using RecipeBook.Data.Entities;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Async repository for <see cref="RecipeIngredient"/> entities.
    /// </summary>
    public class RecipeIngredientRepository(MealPlannerDbContext dbContext) 
        : IRecipeIngredientRepository
    {
        protected readonly MealPlannerDbContext DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        /// <summary>
        /// Gets all recipe ingredients, including their units.
        /// </summary>
        public async Task<IReadOnlyList<RecipeIngredient>> GetAllAsync(
            CancellationToken cancellationToken)
        {
            return await DbContext.RecipeIngredients
                .Include(x => x.Unit)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets all recipe ingredients for a given product id.
        /// </summary>
        public async Task<IReadOnlyList<RecipeIngredient>> SearchAsync(
            Guid productId,
            CancellationToken cancellationToken)
        {
            return await DbContext.RecipeIngredients
                .Include(x => x.Unit)
                .Where(x => x.ProductId == productId)
                .ToListAsync(cancellationToken);
        }
    }
}