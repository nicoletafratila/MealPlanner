using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Async repository for <see cref="Product"/> entities.
    /// </summary>
    public class ProductRepository(MealPlannerDbContext dbContext) : BaseAsyncRepository<Product, int>(dbContext), IProductRepository
    {
        private MealPlannerDbContext Context => (MealPlannerDbContext)DbContext;

        public override async Task<IReadOnlyList<Product>> GetAllAsync()
        {
            return await Context.Products
                .Include(x => x.ProductCategory)
                .Include(x => x.BaseUnit)
                .ToListAsync();
        }

        public override async Task<Product?> GetByIdAsync(int id)
        {
            return await Context.Products
                .Include(x => x.ProductCategory)
                .Include(x => x.BaseUnit)
                .FirstOrDefaultAsync(item => item.Id == id);
        }

        public async Task<IReadOnlyList<Product>> SearchAsync(int categoryId)
        {
            return await Context.Products
                .Include(x => x.ProductCategory)
                .Include(x => x.BaseUnit)
                .Where(x => x.ProductCategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<Product?> SearchAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return await Context.Products
                .Include(x => x.ProductCategory)
                .Include(x => x.BaseUnit)
                .FirstOrDefaultAsync(x =>
                    x.Name != null &&
                    x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}