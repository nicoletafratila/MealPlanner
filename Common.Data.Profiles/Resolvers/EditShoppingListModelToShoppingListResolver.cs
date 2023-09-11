using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class EditShoppingListModelToShoppingListResolver : IMemberValueResolver<EditShoppingListModel, ShoppingList, IList<ShoppingListProductModel>?, IList<ShoppingListProduct>?>
    {
        public IList<ShoppingListProduct>? Resolve(EditShoppingListModel source, ShoppingList destination, IList<ShoppingListProductModel>? sourceValue, IList<ShoppingListProduct>? destValue, ResolutionContext context)
        {
            var result = new List<ShoppingListProduct>();
            if (source.Products != null && source.Products.Any())
            {
                foreach (var item in source.Products)
                {
                    result.Add(context.Mapper.Map<ShoppingListProduct>(item));
                }
            }
            return result;
        }
    }
}
