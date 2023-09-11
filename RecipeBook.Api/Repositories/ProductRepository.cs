using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace RecipeBook.Api.Repositories
{
    public class ProductRepository : BaseAsyncRepository<Product, int>, IProductRepository
    {
        public ProductRepository(MealPlannerDbContext dbContext) : base(dbContext)
        {
        }

        public override async Task<IReadOnlyList<Product>> GetAllAsync()
        {
            return await (DbContext as MealPlannerDbContext)!.Products
                        .Include(x => x.ProductCategory)
                        .Include(x => x.Unit).ToListAsync();
        }

        public override async Task<Product?> GetByIdAsync(int id)
        {
            return await (DbContext as MealPlannerDbContext)!.Products
                    .Include(x => x.ProductCategory)
                    .Include(x => x.Unit)
                    .FirstOrDefaultAsync(item => item.Id == id);
        }

        public async Task<IReadOnlyList<Product>> SearchAsync(int categoryId)
        {
            return await (DbContext as MealPlannerDbContext)!.Products
                    .Include(x => x.ProductCategory)
                    .Include(x => x.Unit)
                    .Where(x => x.ProductCategoryId == categoryId).ToListAsync();
        }

        public async Task<Product?> SearchAsync(string name)
        {
            return await (DbContext as MealPlannerDbContext)!.Products
                    .Include(x => x.ProductCategory)
                    .Include(x => x.Unit)
                    .FirstOrDefaultAsync(x => x.Name!.ToLower() == name.ToLower());
        }
    }
}
