using System.ComponentModel.DataAnnotations.Schema;
using Common.Data.Entities.Converters;

namespace Common.Data.Entities
{
    public class Recipe : Entity<int>
    {
        public string? Name { get; set; }
        public byte[]? ImageContent { get; set; }
        public string? Source { get; set; }

        [ForeignKey("RecipeCategoryId")]
        public RecipeCategory? RecipeCategory { get; set; }
        public int RecipeCategoryId { get; set; }

        public IList<RecipeIngredient>? RecipeIngredients { get; set; }

        public ShoppingList MakeShoppingList(Shop shop)
        {
            ArgumentNullException.ThrowIfNull(shop);

            var list = new ShoppingList
            {
                Name = $"Shopping list details for {Name} in shop {shop.Name}",
                ShopId = shop.Id
            };

            if (RecipeIngredients == null || RecipeIngredients.Count == 0)
            {
                list.Products = [];
                return list;
            }

            var products = new List<ShoppingListProduct>();

            foreach (var item in RecipeIngredients)
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

                    var newProduct = item.ToShoppingListProduct(displaySequence);
                    products.Add(newProduct);
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
