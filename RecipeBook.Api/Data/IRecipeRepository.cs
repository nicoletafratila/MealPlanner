using RecipeBook.Api.Data.Entities;

namespace RecipeBook.Api.Data
{
    public interface IRecipeRepository
    {
        Task<IEnumerable<Recipe>> GetAllAsync();
        Task<Recipe> GetAsync(int id);
        void Add(Recipe entity);
        Task<bool> SaveChangesAsync();
    }
}
