using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Api.Repositories
{
    public class ShopRepository(MealPlannerDbContext dbContext)
        : BaseAsyncRepository<Shop, int>(dbContext), IShopRepository
    {
        private MealPlannerDbContext Context =>
            DbContext as MealPlannerDbContext
            ?? throw new InvalidOperationException("DbContext is not MealPlannerDbContext.");

        public async Task<IReadOnlyList<Shop>> GetAllByUserAsync(
            string userId,
            CancellationToken cancellationToken)
        {
            return await Context.Shops
                .Where(s => s.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<Shop?> GetByIdIncludeDisplaySequenceAsync(
            int? id,
            CancellationToken cancellationToken)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            var ctx = DbContext as MealPlannerDbContext
                      ?? throw new InvalidOperationException("DbContext is not MealPlannerDbContext.");

            return await ctx.Shops
                .Include(x => x.DisplaySequence)!
                    .ThenInclude(x => x.ProductCategory)
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        }
    }
}