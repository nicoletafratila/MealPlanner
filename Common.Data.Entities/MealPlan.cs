using Common.Data.Entities.Converters;
using Common.Data.Entities.Resources;

namespace Common.Data.Entities
{
    public sealed class MealPlan : Entity<int>
    {
        public string? UserId { get; set; }

        public string? Name { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public IList<MealPlanRecipe>? MealPlanRecipes { get; set; } = [];

        public MealPlan()
        {
        }

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
                    Name = string.Format(EntityMessages.ShoppingListNameFormat, Name, shop.Name),
                    ShopId = shop.Id,
                    Products = []
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
                        continue;

                    if (ingredient.Product is null)
                        throw new InvalidOperationException(EntityMessages.IngredientProductMustNotBeNull);

                    if (ingredient.Product.BaseUnit is null)
                        throw new InvalidOperationException(EntityMessages.IngredientProductBaseUnitMustNotBeNull);

                    if (ingredient.Unit is null)
                        throw new InvalidOperationException(EntityMessages.IngredientUnitMustNotBeNull);

                    var productId = ingredient.ProductId;

                    if (!productsById.TryGetValue(productId, out var existingProduct))
                    {
                        var categoryId = ingredient.Product.ProductCategory?.Id;
                        var displaySequence = shop.GetDisplaySequence(categoryId);
                        var sequenceValue = displaySequence?.Value ?? 0;

                        var newProduct = ingredient.ToShoppingListProduct(sequenceValue);
                        productsById.Add(productId, newProduct);
                    }
                    else
                    {
                        var baseUnit = existingProduct.Product?.BaseUnit ?? ingredient.Product.BaseUnit;

                        var additionalQuantity = UnitConverter.Convert(
                            ingredient.Quantity,
                            ingredient.Unit,
                            baseUnit);

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
                Name = string.Format(EntityMessages.ShoppingListNameFormat, Name, shop.Name),
                ShopId = shop.Id,
                Products = productsById.Values.ToList()
            };
        }
    }
}