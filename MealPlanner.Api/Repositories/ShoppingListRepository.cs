﻿using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Api.Repositories
{
    public class ShoppingListRepository : BaseAsyncRepository<ShoppingList, int>, IShoppingListRepository
    {
        public ShoppingListRepository(MealPlannerDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<ShoppingList?> GetByIdAsyncIncludeProductsAsync(int id)
        {
            return await (DbContext as MealPlannerDbContext)!.ShoppingLists
                 .Include(x => x!.Products)!
                    .ThenInclude(x => x.Product)
                        .ThenInclude(x => x!.Unit)
                .FirstOrDefaultAsync(item => item.Id == id);
        }
    }
}