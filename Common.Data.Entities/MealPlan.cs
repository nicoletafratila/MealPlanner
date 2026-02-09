using Common.Data.Entities.Converters;

namespace Common.Data.Entities
{
    public class MealPlan : Entity<int>
    {
        public string? Name { get; set; }
        public IList<MealPlanRecipe>? MealPlanRecipes { get; set; }

        public ShoppingList MakeShoppingList(Shop? shop)
        {
            if (shop is null)
            {
                return new ShoppingList();
            }

            if (MealPlanRecipes == null || !MealPlanRecipes.Any())
            {
                return new ShoppingList
                {
                    Name = $"Shopping list details for {Name} in shop {shop.Name}",
                    ShopId = shop.Id,
                    Products = new List<ShoppingListProduct>()
                };
            }

            var productsById = new Dictionary<int, ShoppingListProduct>();
            foreach (var mealPlanRecipe in MealPlanRecipes)
            {
                var recipe = mealPlanRecipe?.Recipe;
                var ingredients = recipe?.RecipeIngredients;
                if (ingredients == null)
                {
                    continue;
                }

                foreach (var ingredient in ingredients)
                {
                    if (ingredient == null)
                    {
                        continue;
                    }

                    var productId = ingredient.ProductId;
                    if (!productsById.TryGetValue(productId, out var existingProduct))
                    {
                        var categoryId = ingredient.Product?.ProductCategory?.Id;
                        var displaySequence = shop.GetDisplaySequence(categoryId!);
                        
                        var newProduct = ingredient.ToShoppingListProduct(displaySequence.Value);
                        productsById.Add(productId, newProduct);
                    }
                    else
                    {
                        var baseUnit = existingProduct.Product?.BaseUnit;
                        var additionalQuantity = UnitConverter.Convert(
                            ingredient.Quantity,
                            ingredient.Unit!,   
                            baseUnit!
                        );
                        existingProduct.Quantity += additionalQuantity;
                    }
                }
            }

            foreach (var item in productsById.Values)
            {
                item.Product = null;
                item.Unit = null;
            }

            return new ShoppingList
            {
                Name = $"Shopping list details for {Name} in shop {shop.Name}",
                ShopId = shop.Id,
                Products = productsById.Values.ToList()
            };
        }
    }
}
