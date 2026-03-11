using Common.Data.DataContext;
using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Async repository for <see cref="RecipeIngredient"/> entities.
    /// </summary>
    public class RecipeIngredientRepository(MealPlannerDbContext dbContext) : IRecipeIngredientRepository
    {
        private readonly MealPlannerDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        /// <summary>
        /// Gets all recipe ingredients, including their units.
        /// </summary>
        public async Task<IReadOnlyList<RecipeIngredient>> GetAllAsync(
            CancellationToken cancellationToken)
        {
            return await _dbContext.RecipeIngredients
                .Include(x => x.Unit)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets all recipe ingredients for a given product id.
        /// </summary>
        public async Task<IReadOnlyList<RecipeIngredient>> SearchAsync(
            int productId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.RecipeIngredients
                .Where(x => x.ProductId == productId)
                .ToListAsync(cancellationToken);
        }
    }
}