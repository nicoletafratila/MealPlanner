using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Api.Repositories
{
    public class ShoppingListRepository(MealPlannerDbContext dbContext)
        : BaseAsyncRepository<ShoppingList, int>(dbContext), IShoppingListRepository
    {
        public async Task<ShoppingList?> GetByIdIncludeProductsAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var ctx = DbContext as MealPlannerDbContext
                      ?? throw new InvalidOperationException("DbContext is not MealPlannerDbContext.");

            return await ctx.ShoppingLists
                .Include(x => x.Shop)
                .Include(x => x.Products)!
                    .ThenInclude(p => p.Product)!
                        .ThenInclude(p => p!.ProductCategory)
                .Include(x => x.Products)!
                    .ThenInclude(p => p.Product)!
                        .ThenInclude(p => p!.BaseUnit)
                .Include(x => x.Products)!
                    .ThenInclude(p => p.Unit)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<ShoppingList?> SearchAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name must not be null or empty.", nameof(name));

            var ctx = DbContext as MealPlannerDbContext
                      ?? throw new InvalidOperationException("DbContext is not MealPlannerDbContext.");

            return await ctx.ShoppingLists
                .FirstOrDefaultAsync(
                    x => x.Name != null &&
                         x.Name.Equals(name, StringComparison.OrdinalIgnoreCase),
                    cancellationToken);
        }
    }
}