using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Api.Repositories
{
    public class ShoppingListRepository(MealPlannerDbContext dbContext)
        : BaseAsyncRepository<ShoppingList, int>(dbContext), IShoppingListRepository
    {
        private MealPlannerDbContext Ctx =>
            DbContext as MealPlannerDbContext
            ?? throw new InvalidOperationException("DbContext is not MealPlannerDbContext.");

        public async Task<IReadOnlyList<ShoppingList>> GetAllByUserAsync(
            string userId,
            CancellationToken cancellationToken)
        {
            return await Ctx.ShoppingLists
                .Where(sl => sl.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<ShoppingList?> GetByIdIncludeProductsAsync(
            int id,
            CancellationToken cancellationToken)
        {
            return await Ctx.ShoppingLists
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
            string userId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name must not be null or empty.", nameof(name));

            return await Ctx.ShoppingLists
                .FirstOrDefaultAsync(
                    x => x.UserId == userId &&
                         x.Name != null &&
                         x.Name.ToLower() == name.ToLower(),
                    cancellationToken);
        }
    }
}