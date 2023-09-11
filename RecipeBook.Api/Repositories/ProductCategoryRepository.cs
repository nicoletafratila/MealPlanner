using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    public class ProductCategoryRepository : BaseAsyncRepository<ProductCategory, int>, IProductCategoryRepository
    {
        public ProductCategoryRepository(MealPlannerDbContext dbContext) : base(dbContext)
        {
        }
    }
}
