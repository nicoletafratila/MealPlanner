using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class ShoppingListToEditShoppingListModelResolver : IMemberValueResolver<ShoppingList, EditShoppingListModel, IList<ShoppingListProduct>?, IList<ShoppingListProductModel>?>
    {
        public IList<ShoppingListProductModel>? Resolve(ShoppingList source, EditShoppingListModel destination, IList<ShoppingListProduct>? sourceValue, IList<ShoppingListProductModel>? destValue, ResolutionContext context)
        {
            return source.Products!.Select(context.Mapper.Map<ShoppingListProductModel>).OrderBy(item => item.Collected)
                         .ThenBy(item => item.DisplaySequence)
                         .ThenBy(item => item.Product!.Name).ToList();
        }
    }
}
