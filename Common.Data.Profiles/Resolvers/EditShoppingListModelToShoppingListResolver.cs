using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class EditShoppingListModelToShoppingListResolver : IMemberValueResolver<EditShoppingListModel, ShoppingList, IList<ShoppingListProductModel>?, IList<ShoppingListProduct>?>
    {
        public IList<ShoppingListProduct>? Resolve(EditShoppingListModel source, ShoppingList destination, IList<ShoppingListProductModel>? sourceValue, IList<ShoppingListProduct>? destValue, ResolutionContext context)
        {
            return source.Products?.Select(context.Mapper.Map<ShoppingListProduct>).ToList();
        }
    }
}
