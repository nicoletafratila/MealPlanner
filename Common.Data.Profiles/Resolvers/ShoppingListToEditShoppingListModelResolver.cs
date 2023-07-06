using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class ShoppingListToEditShoppingListModelResolver : IMemberValueResolver<ShoppingList, EditShoppingListModel, IList<ShoppingListProduct>?, IList<ShoppingListProductModel>?>
    {
        public IList<ShoppingListProductModel>? Resolve(ShoppingList source, EditShoppingListModel destination, IList<ShoppingListProduct>? sourceValue, IList<ShoppingListProductModel>? destValue, ResolutionContext context)
        {
            var result = new List<ShoppingListProductModel>();
            if (source.Products == null || !source.Products.Any())
                return result;

            foreach (var item in source.Products)
            {
                result.Add(context.Mapper.Map<ShoppingListProductModel>(item));
            }
            return result;
            //return result.OrderBy(item => item.Product!.IngredientCategory!.DisplaySequence)
            //             .ThenBy(item => item.Product!.Name).ToList();
        }
    }
}
