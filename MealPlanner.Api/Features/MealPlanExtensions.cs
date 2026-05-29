using Common.Services.Converters.Resources;
using Common.Services.Converters;
using Common.Services;

namespace MealPlanner.Api.Features
{
    public static class MealPlanExtensions
    {
        public static MealPlanner.Data.Entities.ShoppingList MakeShoppingList(
            this MealPlanner.Data.Entities.MealPlan mealPlan,
            MealPlanner.Data.Entities.Shop? shop)
        {
            if (shop is null)
                return new MealPlanner.Data.Entities.ShoppingList();

            if (mealPlan.MealPlanRecipes == null || !mealPlan.MealPlanRecipes.Any())
            {
                return new MealPlanner.Data.Entities.ShoppingList
                {
                    Name = string.Format(ConverterMessages.ShoppingListNameFormat, mealPlan.Name, shop.Name),
                    ShopId = shop.Id,
                    Products = []
                };
            }

            var productsById = new Dictionary<int, MealPlanner.Data.Entities.ShoppingListProduct>();

            foreach (var mealPlanRecipe in mealPlan.MealPlanRecipes)
            {
                var recipeEntity = mealPlanRecipe?.Recipe;
                var ingredients = recipeEntity?.RecipeIngredients;
                if (ingredients == null)
                    continue;

                foreach (var ingredient in ingredients)
                {
                    if (ingredient == null)
                        continue;

                    if (ingredient.Product is null)
                        throw new InvalidOperationException(ConverterMessages.IngredientProductMustNotBeNull);

                    if (ingredient.Product.BaseUnit is null)
                        throw new InvalidOperationException(ConverterMessages.IngredientProductBaseUnitMustNotBeNull);

                    if (ingredient.Unit is null)
                        throw new InvalidOperationException(ConverterMessages.IngredientUnitMustNotBeNull);

                    var productId = ingredient.ProductId;

                    if (!productsById.TryGetValue(productId, out var existingProduct))
                    {
                        var categoryId = ingredient.Product.ProductCategory?.Id;
                        var sequenceValue = shop.GetDisplaySequence(categoryId)?.Value ?? 0;
                        productsById.Add(productId, ingredient.ToShoppingListProduct(sequenceValue));
                    }
                    else
                    {
                        var baseUnit = existingProduct.Product?.BaseUnit ?? ingredient.Product.BaseUnit;
                        existingProduct.Quantity += UnitConverter.Convert(ingredient.Quantity, ingredient.Unit, baseUnit);
                    }
                }
            }

            foreach (var item in productsById.Values)
            {
                item.Product = null;
                item.Unit = null;
            }

            return new MealPlanner.Data.Entities.ShoppingList
            {
                Name = string.Format(ConverterMessages.ShoppingListNameFormat, mealPlan.Name, shop.Name),
                ShopId = shop.Id,
                Products = productsById.Values.ToList()
            };
        }
    }
}
