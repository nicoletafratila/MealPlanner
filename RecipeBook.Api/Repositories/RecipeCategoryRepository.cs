using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace RecipeBook.Api.Repositories
{
    public class RecipeCategoryRepository(MealPlannerDbContext dbContext) : BaseAsyncRepository<RecipeCategory, int>(dbContext), IRecipeCategoryRepository
    {
        public async Task UpdateAllAsync(IList<RecipeCategory> entities)
        {
            foreach (var entity in entities)
            {
                DbContext.Entry(entity).State = EntityState.Modified;
            }
            await DbContext.SaveChangesAsync();
        }
    }
}
