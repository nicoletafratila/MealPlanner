﻿using MealPlanner.Shared.Models;
using MediatR;

namespace RecipeBook.Api.Features.Recipe.Queries.GetShoppingListProducts
{
    public class GetShoppingListProductsQuery : IRequest<IList<ShoppingListProductModel>?>
    {
        public int RecipeId { get; set; }
        public int ShopId { get; set; }
    }
}
