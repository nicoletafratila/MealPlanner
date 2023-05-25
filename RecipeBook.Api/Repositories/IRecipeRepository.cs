using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    public interface IRecipeRepository : IAsyncRepository<Recipe, int>
    {
        Task<IReadOnlyList<Recipe>> SearchAsync(int categoryId);
        Task<Recipe?> GetByIdIncludeIngredientsAsync(int id);
    }
}
