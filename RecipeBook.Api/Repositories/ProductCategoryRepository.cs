using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Async repository for <see cref="ProductCategory"/> entities.
    /// </summary>
    public class ProductCategoryRepository(MealPlannerDbContext dbContext) : BaseAsyncRepository<ProductCategory, int>(dbContext), IProductCategoryRepository
    {
    }
}