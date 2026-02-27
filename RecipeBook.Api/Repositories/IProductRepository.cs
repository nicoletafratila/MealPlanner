using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    /// <summary>
    /// Repository contract for querying and manipulating <see cref="Product"/> entities.
    /// </summary>
    public interface IProductRepository : IAsyncRepository<Product, int>
    {
        /// <summary>
        /// Gets all products in a given category.
        /// </summary>
        Task<IReadOnlyList<Product>> SearchAsync(int categoryId);

        /// <summary>
        /// Finds a product by name (case-insensitive), or null if not found.
        /// </summary>
        Task<Product?> SearchAsync(string name);
    }
}