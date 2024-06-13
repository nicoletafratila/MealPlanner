using Common.Data.Entities.Converters;

namespace Common.Data.Entities
{
    public class MealPlan : Entity<int>
    {
        public string? Name { get; set; }
        public IList<MealPlanRecipe>? MealPlanRecipes { get; set; }

        public ShoppingList MakeShoppingList(Shop shop)
        {
            var list = new ShoppingList
            {
                Name = $"Shopping list details for {Name} in shop {shop.Name}",
                ShopId = shop.Id
            };

            var products = new List<ShoppingListProduct>();
            foreach (var item in MealPlanRecipes!)
            {
                foreach (var i in item.Recipe?.RecipeIngredients!)
                {
                    var existingProduct = products.FirstOrDefault(x => x.ProductId == i.ProductId);
                    if (existingProduct == null)
                    {
                        var displaySequence = shop?.GetDisplaySequence(i.Product?.ProductCategory?.Id);
                        var newProduct = i.ToShoppingListProduct(displaySequence!.Value);
                        products.Add(newProduct);
                    }
                    else
                        existingProduct.Quantity += UnitConverter.Convert(i.Quantity, existingProduct.Unit!, existingProduct!.Product!.BaseUnit!);  
                }
            }
            list.Products = products;
            return list;
        }
    }
}
