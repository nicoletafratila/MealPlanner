using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Repository contract for querying and manipulating <see cref="ProductCategory"/> entities.
    /// </summary>
    public interface IProductCategoryRepository : IAsyncRepository<ProductCategory, int>
    {
    }
}