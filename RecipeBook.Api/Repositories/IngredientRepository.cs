using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace RecipeBook.Api.Repositories
{
    public class IngredientRepository : BaseAsyncRepository<Ingredient, int>, IIngredientRepository
    {
        public IngredientRepository(MealPlannerDbContext dbContext) : base(dbContext)
        {
        }

        public override async Task<IReadOnlyList<Ingredient>> GetAllAsync()
        {
            return await (DbContext as MealPlannerDbContext)!.Ingredients
                        .Include(x => x.IngredientCategory)
                        .Include(x => x.Unit).ToListAsync();
        }

        public override async Task<Ingredient?> GetByIdAsync(int id)
        {
            return await (DbContext as MealPlannerDbContext)!.Ingredients
                    .Include(x => x.IngredientCategory)
                    .Include(x => x.Unit)
                    .FirstOrDefaultAsync(item => item.Id == id);
        }

        public async Task<IReadOnlyList<Ingredient>> SearchAsync(int categoryId)
        {
            return await (DbContext as MealPlannerDbContext)!.Ingredients
                    .Include(x => x.IngredientCategory)
                    .Include(x => x.Unit)
                    .Where(x => x.IngredientCategoryId == categoryId).ToListAsync();
        }

        public async Task<Ingredient?> SearchAsync(string name)
        {
            return await (DbContext as MealPlannerDbContext)!.Ingredients
                    .Include(x => x.IngredientCategory)
                    .Include(x => x.Unit)
                    .FirstOrDefaultAsync(x => x.Name!.ToLower() == name.ToLower());
        }
    }
}
