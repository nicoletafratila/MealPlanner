using Common.Data.DataContext;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;
using RecipeBook.Data.Entities;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Async repository for <see cref="RecipeCategory"/> entities.
    /// </summary>
    public class RecipeCategoryRepository(MealPlannerDbContext dbContext)
        : BaseAsyncRepository<RecipeCategory, Guid>(dbContext), IRecipeCategoryRepository
    {
        private MealPlannerDbContext Context => (MealPlannerDbContext)DbContext;

        public async Task<IReadOnlyList<RecipeCategory>> GetAllByUserAsync(
            string userId,
            CancellationToken cancellationToken)
        {
            return await Context.RecipeCategories
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.DisplaySequence)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<RecipeCategory>> GetByIdsAsync(
            IList<Guid> ids,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ids);

            return await Context.RecipeCategories
                .Where(c => ids.Contains(c.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateAllAsync(
            IList<RecipeCategory> entities,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(entities);

            if (entities.Count == 0)
                return;

            foreach (var entity in entities)
                Context.Entry(entity).Property(c => c.DisplaySequence).IsModified = true;

            await Context.SaveChangesAsync(cancellationToken);
        }
    }
}