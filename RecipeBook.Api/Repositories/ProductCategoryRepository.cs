using Common.Data.DataContext;
using Common.Data.Repository;
using Common.Pagination;
using Microsoft.EntityFrameworkCore;
using RecipeBook.Data.Entities;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Async repository for <see cref="ProductCategory"/> entities.
    /// </summary>
    public class ProductCategoryRepository(MealPlannerDbContext dbContext)
        : BaseAsyncRepository<ProductCategory, Guid>(dbContext), IProductCategoryRepository
    {
        private MealPlannerDbContext Context => (MealPlannerDbContext)DbContext;

        public async Task<IReadOnlyList<ProductCategory>> GetAllByUserAsync(
            string userId,
            CancellationToken cancellationToken)
        {
            return await Context.ProductCategories
                .Where(c => c.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedQueryResult<ProductCategory>> SearchByUserAsync(
            string userId,
            IEnumerable<FilterItem>? filters,
            IEnumerable<SortingModel>? sorting,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            IQueryable<ProductCategory> query = Context.ProductCategories
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Id);

            var filtered = query.ApplyFilters(filters).ApplySorting(sorting);

            return await filtered.ToPagedResultAsync(pageNumber, pageSize, cancellationToken);
        }
    }
}