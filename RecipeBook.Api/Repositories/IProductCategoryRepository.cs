using Common.Data.Repository;
using RecipeBook.Data.Entities;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Repository contract for querying and manipulating <see cref="ProductCategory"/> entities.
    /// </summary>
    public interface IProductCategoryRepository : IAsyncRepository<ProductCategory, Guid>
    {
        Task<IReadOnlyList<ProductCategory>> GetAllByUserAsync(string userId, CancellationToken cancellationToken);
    }
}