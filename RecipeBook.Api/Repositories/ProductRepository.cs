using Common.Data.DataContext;
using Common.Data.Repository;
using Common.Pagination;
using Microsoft.EntityFrameworkCore;
using RecipeBook.Data.Entities;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Async repository for <see cref="Product"/> entities.
    /// </summary>
    public class ProductRepository(MealPlannerDbContext dbContext)
        : BaseAsyncRepository<Product, Guid>(dbContext), IProductRepository
    {
        private MealPlannerDbContext Context => (MealPlannerDbContext)DbContext;

        public async Task<IReadOnlyList<Product>> GetAllByUserAsync(
            string userId,
            CancellationToken cancellationToken)
        {
            return await Context.Products
                .Include(x => x.ProductCategory)
                .Include(x => x.BaseUnit)
                .Where(x => x.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public override async Task<IReadOnlyList<Product>> GetAllAsync(
            CancellationToken cancellationToken)
        {
            return await Context.Products
                .Include(x => x.ProductCategory)
                .Include(x => x.BaseUnit)
                .ToListAsync(cancellationToken);
        }

        public override async Task<Product?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return await Context.Products
                .Include(x => x.ProductCategory)
                .Include(x => x.BaseUnit)
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> SearchAsync(
            Guid categoryId,
            CancellationToken cancellationToken)
        {
            return await Context.Products
                .Include(x => x.ProductCategory)
                .Include(x => x.BaseUnit)
                .Where(x => x.ProductCategoryId == categoryId)
                .ToListAsync(cancellationToken);
        }

        public async Task<Product?> SearchAsync(
            string name,
            string userId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return await Context.Products
                .Include(x => x.ProductCategory)
                .Include(x => x.BaseUnit)
                .FirstOrDefaultAsync(
                    x => x.UserId == userId && x.Name != null && x.Name.ToLower() == name.ToLower(),
                    cancellationToken);
        }

        public async Task<PagedQueryResult<Product>> SearchByUserAsync(
            string userId,
            Guid? categoryId,
            IEnumerable<FilterItem>? filters,
            IEnumerable<SortingModel>? sorting,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken,
            bool thumbnailOnly = false)
        {
            IQueryable<Product> query = Context.Products
                .Include(x => x.ProductCategory)
                .Include(x => x.BaseUnit)
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.Id);

            if (categoryId is not null)
                query = query.Where(x => x.ProductCategoryId == categoryId.Value);

            var remappedFilters = filters.RemapPropertyName("ProductCategoryName", "ProductCategory.Name");
            var remappedSorting = sorting.RemapPropertyName("ProductCategoryName", "ProductCategory.Name");
            var filtered = query.ApplyFilters(remappedFilters).ApplySorting(remappedSorting);

            if (thumbnailOnly)
            {
                filtered = filtered.Select(x => new Product
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Name = x.Name,
                    ImageThumbnail = x.ImageThumbnail,
                    BaseUnit = x.BaseUnit,
                    BaseUnitId = x.BaseUnitId,
                    ProductCategory = x.ProductCategory,
                    ProductCategoryId = x.ProductCategoryId
                });
            }

            return await filtered.ToPagedResultAsync(pageNumber, pageSize, cancellationToken);
        }
    }
}