using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public class Recipe : Entity<int>
    {
        public string? Name { get; set; }
        public byte[]? ImageContent { get; set; }

        [ForeignKey("RecipeCategoryId")]
        public RecipeCategory? RecipeCategory { get; private set; }
        public int RecipeCategoryId { get; set; }

        public IList<RecipeIngredient>? RecipeIngredients { get; set; }

        public ShoppingList MakeShoppingList(Shop shop)
        {
            var list = new ShoppingList
            {
                Name = $"Shopping list details for {Name} in shop {shop.Name}",
                ShopId = shop.Id
            };
            var products = new List<ShoppingListProduct>();
            foreach (var item in RecipeIngredients!)
            {
                var existingProduct = products.FirstOrDefault(x => x.ProductId == item.ProductId);
                if (existingProduct == null)
                {
                    var displaySequence = shop?.GetDisplaySequence(item.Product?.ProductCategory?.Id);
                    var newProduct = item.ToShoppingListProduct(displaySequence!.Value);
                    products.Add(newProduct);
                }
                else
                    existingProduct.Quantity += item.Quantity;
            }
            list.Products = products;
            return list;
        }
    }
}
