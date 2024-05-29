using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class EditShoppingListModelToShoppingListResolver : IMemberValueResolver<ShoppingListEditModel, ShoppingList, IList<ShoppingListProductEditModel>?, IList<ShoppingListProduct>?>
    {
        public IList<ShoppingListProduct>? Resolve(ShoppingListEditModel source, ShoppingList destination, IList<ShoppingListProductEditModel>? sourceValue, IList<ShoppingListProduct>? destValue, ResolutionContext context)
        {
            return source.Products?.Select(context.Mapper.Map<ShoppingListProduct>).ToList();
        }
    }
}
