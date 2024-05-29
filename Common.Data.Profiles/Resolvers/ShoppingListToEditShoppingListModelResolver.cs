using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class ShoppingListToEditShoppingListModelResolver : IMemberValueResolver<ShoppingList, ShoppingListEditModel, IList<ShoppingListProduct>?, IList<ShoppingListProductEditModel>?>
    {
        public IList<ShoppingListProductEditModel>? Resolve(ShoppingList source, ShoppingListEditModel destination, IList<ShoppingListProduct>? sourceValue, IList<ShoppingListProductEditModel>? destValue, ResolutionContext context)
        {
            return source.Products?.Select(context.Mapper.Map<ShoppingListProductEditModel>)
                         .OrderBy(item => item.Collected)
                         .ThenBy(item => item.DisplaySequence)
                         .ThenBy(item => item.Product?.Name).ToList();
        }
    }
}
