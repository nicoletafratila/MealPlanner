using Common.Data.Entities;
using Common.Services;
using Common.Services.Converters;
using Common.Services.Converters.Resources;

namespace RecipeBook.Api.Features
{
    public static class RecipeExtensions
    {
        public static ShoppingList MakeShoppingList(this Common.Data.Entities.Recipe recipe, Shop shop)
        {
            ArgumentNullException.ThrowIfNull(shop);

            var list = new ShoppingList
            {
                Name = string.Format(ConverterMessages.ShoppingListNameFormat, recipe.Name, shop.Name),
                ShopId = shop.Id
            };

            if (recipe.RecipeIngredients == null || recipe.RecipeIngredients.Count == 0)
            {
                list.Products = [];
                return list;
            }

            var products = new List<ShoppingListProduct>();

            foreach (var item in recipe.RecipeIngredients)
            {
                if (item == null)
                    continue;

                if (item.Product == null || item.Product.BaseUnit == null)
                    continue;

                var existingProduct = products.FirstOrDefault(x => x.ProductId == item.ProductId);
                if (existingProduct == null)
                {
                    var categoryId = item.Product.ProductCategory?.Id;
                    var displaySequence = shop.GetDisplaySequence(categoryId) != null ? shop.GetDisplaySequence(categoryId)!.Value : 1;
                    products.Add(item.ToShoppingListProduct(displaySequence));
                }
                else
                {
                    if (item.Unit == null || existingProduct.Product?.BaseUnit == null)
                        continue;

                    existingProduct.Quantity += UnitConverter.Convert(
                        item.Quantity,
                        item.Unit,
                        existingProduct.Product.BaseUnit);
                }
            }

            list.Products = products;
            return list;
        }
    }
}
