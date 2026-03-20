using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;

namespace Common.Data.Profiles.Resolvers
{
    public class EditShoppingListModelToShoppingListResolver(IMapper mapper)
        : IMemberValueResolver<
            ShoppingListEditModel,
            ShoppingList,
            IList<ShoppingListProductEditModel>?,
            IList<ShoppingListProduct>?>
    {
        public IList<ShoppingListProduct>? Resolve(
            ShoppingListEditModel source,
            ShoppingList destination,
            IList<ShoppingListProductEditModel>? sourceValue,
            IList<ShoppingListProduct>? destValue,
            ResolutionContext context)
        {
            if (source.Products == null || source.Products.Count == 0)
                return [];

            return source.Products
                .Select(p => mapper.Map<ShoppingListProduct>(p))
                .ToList();
        }
    }
}