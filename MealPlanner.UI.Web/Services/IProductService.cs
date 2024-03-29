﻿using Common.Pagination;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IProductService
    {
        Task<EditProductModel?> GetEditAsync(int id);
        Task<PagedList<ProductModel>?> SearchAsync(string? categoryId = null, QueryParameters? queryParameters = null);
        Task<string?> AddAsync(EditProductModel model);
        Task<string?> UpdateAsync(EditProductModel model);
        Task<string?> DeleteAsync(int id);
    }
}
