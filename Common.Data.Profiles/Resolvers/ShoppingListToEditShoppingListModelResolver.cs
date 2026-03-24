using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class ShoppingListToEditShoppingListModelResolver()
        : IMemberValueResolver<
            ShoppingList,
            ShoppingListEditModel,
            IList<ShoppingListProduct>?,
            IList<ShoppingListProductEditModel>?>
    {
        public IList<ShoppingListProductEditModel>? Resolve(
            ShoppingList source,
            ShoppingListEditModel destination,
            IList<ShoppingListProduct>? sourceValue,
            IList<ShoppingListProductEditModel>? destValue,
            ResolutionContext context)
        {
            if (source.Products == null || source.Products.Count == 0)
                return [];

            return source.Products
                .Select(p => context.Mapper.Map<ShoppingListProductEditModel>(p))
                .OrderBy(i => i.Collected)
                .ThenBy(i => i.DisplaySequence)
                .ThenBy(i => i.Product?.Name)
                .ToList();
        }
    }
}