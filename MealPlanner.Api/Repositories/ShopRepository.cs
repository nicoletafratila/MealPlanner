using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Api.Repositories
{
    public class ShopRepository : BaseAsyncRepository<Shop, int>, IShopRepository
    {
        public ShopRepository(MealPlannerDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Shop?> GetByIdIncludeDisplaySequenceAsync(int id)
        {
            return await (DbContext as MealPlannerDbContext)!.Shops
              .Include(x => x.DisplaySequence)
              .FirstOrDefaultAsync(item => item.Id == id);
        }
    }
}
