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
        private readonly MealPlannerDbContext DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        private MealPlannerDbContext Context => (MealPlannerDbContext)DbContext;

        public async Task<IReadOnlyList<RecipeIngredient>> GetAllAsync(
            CancellationToken cancellationToken)
        {
            return await Context.RecipeIngredients
                .Include(x => x.Unit)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<RecipeIngredient>> SearchAsync(
            Guid productId,
            CancellationToken cancellationToken)
        {
            return await Context.RecipeIngredients
                .Include(x => x.Unit)
                .Where(x => x.ProductId == productId)
                .ToListAsync(cancellationToken);
        }
    }
}