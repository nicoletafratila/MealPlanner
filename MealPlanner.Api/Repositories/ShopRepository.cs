using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Api.Repositories
{
    public class ShopRepository(MealPlannerDbContext dbContext) : BaseAsyncRepository<Shop, int>(dbContext), IShopRepository
    {
        public async Task<Shop?> GetByIdIncludeDisplaySequenceAsync(int id)
        {
            return await (DbContext as MealPlannerDbContext)!.Shops
              .Include(x => x.DisplaySequence)!
              .ThenInclude(x => x.ProductCategory)
              .FirstOrDefaultAsync(item => item.Id == id);
        }
    }
}
