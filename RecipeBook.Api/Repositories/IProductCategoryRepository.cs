using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Repository contract for querying and manipulating <see cref="ProductCategory"/> entities.
    /// </summary>
    public interface IProductCategoryRepository : IAsyncRepository<ProductCategory, int>
    {
        Task<IReadOnlyList<ProductCategory>> GetAllByUserAsync(string userId, CancellationToken cancellationToken);
    }
}