using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Async repository for <see cref="Product"/> entities.
    /// </summary>
    public class ProductRepository(MealPlannerDbContext dbContext)
        : BaseAsyncRepository<Product, int>(dbContext), IProductRepository
    {
        private MealPlannerDbContext Context => (MealPlannerDbContext)DbContext;

        public override async Task<IReadOnlyList<Product>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await Context.Products
                .Include(x => x.ProductCategory)
                .Include(x => x.BaseUnit)
                .ToListAsync(cancellationToken);
        }

        public override async Task<Product?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await Context.Products
                .Include(x => x.ProductCategory)
                .Include(x => x.BaseUnit)
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> SearchAsync(
            int categoryId,
            CancellationToken cancellationToken = default)
        {
            return await Context.Products
                .Include(x => x.ProductCategory)
                .Include(x => x.BaseUnit)
                .Where(x => x.ProductCategoryId == categoryId)
                .ToListAsync(cancellationToken);
        }

        public async Task<Product?> SearchAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return await Context.Products
                .Include(x => x.ProductCategory)
                .Include(x => x.BaseUnit)
                .FirstOrDefaultAsync(
                    x => x.Name != null && x.Name.ToLower() == name.ToLower(),
                    cancellationToken);
        }
    }
}