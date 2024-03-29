﻿using Common.Pagination;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IRecipeService
    {
        Task<RecipeModel?> GetByIdAsync(int id);
        Task<EditRecipeModel?> GetEditAsync(int id);
        Task<IList<ShoppingListProductModel>?> GetShoppingListProducts(int recipeId, int shopId);
        Task<PagedList<RecipeModel>?> SearchAsync(string? categoryId = null, QueryParameters? queryParameters = null);
        Task<string?> AddAsync(EditRecipeModel model);
        Task<string?> UpdateAsync(EditRecipeModel model);
        Task<string?> DeleteAsync(int id);
    }
}