using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Api.Repositories
{
    public class ShoppingListRepository(MealPlannerDbContext dbContext) : BaseAsyncRepository<ShoppingList, int>(dbContext), IShoppingListRepository
    {
        public async Task<ShoppingList?> GetByIdIncludeProductsAsync(int id)
        {
            return await (DbContext as MealPlannerDbContext)!.ShoppingLists
                 .Include(x => x!.Shop)!
                 .Include(x => x!.Products)!
                    .ThenInclude(x => x!.Product)
                        .ThenInclude(x => x!.ProductCategory)
                 .Include(x => x!.Products)!
                    .ThenInclude(x => x!.Product)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ShoppingList?> SearchAsync(string name)
        {
            return await (DbContext as MealPlannerDbContext)!.ShoppingLists
                   .FirstOrDefaultAsync(x => x!.Name!.ToLower() == name.ToLower());
        }
    }
}
