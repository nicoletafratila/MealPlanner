using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    public interface IProductRepository : IAsyncRepository<Product, int>
    {
        Task<IReadOnlyList<Product>> SearchAsync(int categoryId);
        Task<Product?> SearchAsync(string name);
    }
}
