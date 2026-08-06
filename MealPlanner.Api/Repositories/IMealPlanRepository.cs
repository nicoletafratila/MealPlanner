using Common.Data.Entities;
using Common.Data.Repository;
using Common.Pagination;
using MealPlanner.Data.Entities;

namespace MealPlanner.Api.Repositories
{
    public interface IMealPlanRepository : IAsyncRepository<MealPlan, Guid>
    {
        Task<IReadOnlyList<MealPlan>> GetAllByUserAsync(string userId, CancellationToken cancellationToken);
        Task<MealPlan?> GetByIdIncludeRecipesAsync(Guid id, CancellationToken cancellationToken);
        Task<IList<CategoryItemCount>> SearchByRecipeCategoryIdsAsync(IList<Guid> categoryIds, string userId, CancellationToken cancellationToken);
        Task<IList<CategoryItemCount>> SearchByProductCategoryIdsAsync(IList<Guid> categoryIds, string userId, CancellationToken cancellationToken);
        Task<IList<MealPlan>> SearchByRecipeAsync(Guid recipeId, string userId, CancellationToken cancellationToken);
        Task<MealPlan?> SearchAsync(string name, string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Filters, sorts, and pages meal plans for a user at the database level, returning only the requested page.
        /// </summary>
        Task<PagedQueryResult<MealPlan>> SearchByUserAsync(
            string userId,
            IEnumerable<FilterItem>? filters,
            IEnumerable<SortingModel>? sorting,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken);
    }
}
