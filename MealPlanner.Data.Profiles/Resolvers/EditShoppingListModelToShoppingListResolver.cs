using AutoMapper;
using MealPlanner.Data.Entities;
using MealPlanner.Shared.Models;

namespace MealPlanner.Data.Profiles.Resolvers
{
    public class EditShoppingListModelToShoppingListResolver()
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
                .Select(p =>
                {
                    var productId = p.Product?.Id ?? Guid.Empty;
                    var existing = destValue?.FirstOrDefault(d => d.ProductId == productId);
                    if (existing != null)
                    {
                        context.Mapper.Map(p, existing);
                        return existing;
                    }
                    return context.Mapper.Map<ShoppingListProduct>(p);
                })
                .ToList();
        }
    }
}
