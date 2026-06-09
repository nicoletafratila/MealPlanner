using Common.Data.DataContext;
using Common.Data.Repository;
using MealPlanner.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Api.Repositories
{
    public class ShopRepository(MealPlannerDbContext dbContext)
        : BaseAsyncRepository<Shop, Guid>(dbContext), IShopRepository
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

        public override async Task UpdateAsync(Shop entity, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(entity);

            Context.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                var desired = entity.DisplaySequence ?? [];

                var existing = await Context.ShopDisplaySequences
                    .Where(s => s.ShopId == entity.Id)
                    .ToListAsync(cancellationToken);
                var desiredCategoryIds = desired.Select(s => s.ProductCategoryId).ToHashSet();

                Context.ShopDisplaySequences.RemoveRange(
                    existing.Where(s => !desiredCategoryIds.Contains(s.ProductCategoryId)));

                var existingByCategory = existing.ToDictionary(s => s.ProductCategoryId);
                var toAdd = new List<ShopDisplaySequence>();
                foreach (var item in desired)
                {
                    if (existingByCategory.TryGetValue(item.ProductCategoryId, out var tracked))
                    {
                        tracked.Value = item.Value;
                        Context.Entry(tracked).Property(s => s.Value).IsModified = true;
                    }
                    else
                    {
                        toAdd.Add(new ShopDisplaySequence { ShopId = entity.Id, ProductCategoryId = item.ProductCategoryId, Value = item.Value });
                    }
                }
                await Context.ShopDisplaySequences.AddRangeAsync(toAdd, cancellationToken);

                entity.DisplaySequence = existing
                    .Where(s => desiredCategoryIds.Contains(s.ProductCategoryId))
                    .Concat(toAdd)
                    .ToList();

                Context.Entry(entity).State = EntityState.Modified;
                await Context.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                Context.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        public async Task<Shop?> GetByIdIncludeDisplaySequenceAsync(
            Guid? id,
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