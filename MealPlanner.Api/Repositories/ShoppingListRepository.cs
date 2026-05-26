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

        public override async Task UpdateAsync(ShoppingList entity, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var existing = await Ctx.ShoppingListProducts
                .Where(p => p.ShoppingListId == entity.Id)
                .ToListAsync(cancellationToken);

            var desired = entity.Products ?? [];
            var desiredProductIds = desired.Select(p => p.ProductId).ToHashSet();

            Ctx.ShoppingListProducts.RemoveRange(
                existing.Where(p => !desiredProductIds.Contains(p.ProductId)));

            var existingByProduct = existing.ToDictionary(p => p.ProductId);
            var toAdd = new List<ShoppingListProduct>();
            foreach (var item in desired)
            {
                if (existingByProduct.TryGetValue(item.ProductId, out var tracked))
                {
                    tracked.Quantity = item.Quantity;
                    tracked.UnitId = item.UnitId;
                    tracked.Collected = item.Collected;
                    tracked.DisplaySequence = item.DisplaySequence;
                }
                else
                {
                    toAdd.Add(new ShoppingListProduct
                    {
                        ShoppingListId = entity.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitId = item.UnitId,
                        Collected = item.Collected,
                        DisplaySequence = item.DisplaySequence
                    });
                }
            }
            await Ctx.ShoppingListProducts.AddRangeAsync(toAdd, cancellationToken);

            entity.Products = existing
                .Where(p => desiredProductIds.Contains(p.ProductId))
                .Concat(toAdd)
                .ToList();

            Ctx.Entry(entity).State = EntityState.Modified;
            await Ctx.SaveChangesAsync(cancellationToken);
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