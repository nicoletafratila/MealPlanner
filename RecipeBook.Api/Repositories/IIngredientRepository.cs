using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    public interface IIngredientRepository : IAsyncRepository<Ingredient, int>
    {
        Task<IReadOnlyList<Ingredient>> SearchAsync(int categoryId);
        Task<Ingredient?> SearchAsync(string name);
    }
}
