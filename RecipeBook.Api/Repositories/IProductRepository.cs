using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Repository contract for querying and manipulating <see cref="Product"/> entities.
    /// </summary>
    public interface IProductRepository : IAsyncRepository<Product, int>
    {
        Task<IReadOnlyList<Product>> GetAllByUserAsync(string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Gets all products in a given category.
        /// </summary>
        Task<IReadOnlyList<Product>> SearchAsync(int categoryId, CancellationToken cancellationToken);

        /// <summary>
        /// Finds a product by name (case-insensitive) scoped to a user, or null if not found.
        /// </summary>
        Task<Product?> SearchAsync(string name, string userId, CancellationToken cancellationToken);
    }
}