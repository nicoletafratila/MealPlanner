using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    public interface IRecipeRepository : IAsyncRepository<Recipe, int>
    {
        Task<Recipe?> GetByIdIncludeIngredientsAsync(int id);
        Task<IReadOnlyList<Recipe>> SearchAsync(int categoryId);
        Task<Recipe?> SearchAsync(string name);
    }
}
