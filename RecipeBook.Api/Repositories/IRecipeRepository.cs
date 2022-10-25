using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Data.Repositories
{
    public interface IRecipeRepository : IAsyncRepository<Recipe, int>
    {
        Task<Recipe> GetByIdAsyncIncludeIngredients(int id);
    }
}
