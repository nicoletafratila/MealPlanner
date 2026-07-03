using Common.Data.DataContext;
using Common.Data.Repository;
using MealPlanner.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Api.Repositories
{
    public class ShopRepository(MealPlannerDbContext dbContext)
        : BaseAsyncRepository<Shop, Guid>(dbContext), IShopRepository
    {
        private MealPlannerDbContext Context => (MealPlannerDbContext)DbContext;

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

            var existing = await Context.ShopDisplaySequences
                .Where(s => s.ShopId == entity.Id)
                .ToListAsync(cancellationToken);
            Context.ShopDisplaySequences.RemoveRange(existing);

            if (entity.DisplaySequence?.Count > 0)
            {
                foreach (var seq in entity.DisplaySequence)
                    seq.ShopId = entity.Id;

                await Context.ShopDisplaySequences.AddRangeAsync(entity.DisplaySequence, cancellationToken);
            }

            Context.Entry(entity).State = EntityState.Modified;
            await Context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Shop?> GetByIdIncludeDisplaySequenceAsync(
            Guid? id,
            CancellationToken cancellationToken)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            return await Context.Shops
                .Include(x => x.DisplaySequence)!
                    .ThenInclude(x => x.ProductCategory)
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        }
    }
}