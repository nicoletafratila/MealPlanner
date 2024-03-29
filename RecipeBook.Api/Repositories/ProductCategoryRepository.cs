﻿using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Repository;

namespace RecipeBook.Api.Repositories
{
    public class ProductCategoryRepository(MealPlannerDbContext dbContext) : BaseAsyncRepository<ProductCategory, int>(dbContext), IProductCategoryRepository
    {
    }
}
