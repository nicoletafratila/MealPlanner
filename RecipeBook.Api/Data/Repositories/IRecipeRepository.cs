using Common.Data.Entities;
using Common.Repository.Repositories;

namespace RecipeBook.Api.Data.Repositories
{
    public interface IRecipeRepository : IAsyncRepository<Recipe, int>
    {
        Task<Recipe> GetByIdAsyncIncludeIngredients(int id);
    }
}
