namespace Common.Data.Entities.Tests
{
    [TestFixture]
    public class MealPlanTests
    {
        [Test]
        public void MakeShoppingList_Returns_Empty_List_When_Shop_Is_Null()
        {
            var mealPlan = new MealPlan { Name = "Plan" };

            var result = mealPlan.MakeShoppingList(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.ShopId, Is.Zero);
                Assert.That(result.Name, Is.Null);
            }
        }

        [Test]
        public void MakeShoppingList_Returns_Empty_Products_When_No_Recipes()
        {
            var mealPlan = new MealPlan { Name = "Plan" };
            var shop = new Shop { Id = 1, Name = "Shop1" };

            var result = mealPlan.MakeShoppingList(shop);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Name, Is.EqualTo("Shopping list details for Plan in shop Shop1"));
                Assert.That(result.ShopId, Is.EqualTo(1));
                Assert.That(result.Products, Is.Not.Null);
                Assert.That(result.Products, Is.Empty);
            }
        }

        [Test]
        public void MakeShoppingList_Aggregates_Quantities_Per_Product_And_Uses_DisplaySequence()
        {
            // Arrange
            var baseUnit = new Unit { Id = 2, Name = "gr" };
            var category = new ProductCategory { Id = 3, Name = "Category" };
            var product = new Product
            {
                Id = 10,
                Name = "Flour",
                BaseUnit = baseUnit,
                BaseUnitId = baseUnit.Id,
                ProductCategory = category,
                ProductCategoryId = category.Id
            };

            var ingredient1 = new RecipeIngredient
            {
                Product = product,
                ProductId = product.Id,
                Quantity = 100m,
                Unit = baseUnit,
                UnitId = baseUnit.Id
            };

            var ingredient2 = new RecipeIngredient
            {
                Product = product,
                ProductId = product.Id,
                Quantity = 50m,
                Unit = baseUnit,
                UnitId = baseUnit.Id
            };

            var recipe1 = new Recipe
            {
                Id = 1,
                Name = "Recipe1",
                RecipeIngredients = [ingredient1]
            };

            var recipe2 = new Recipe
            {
                Id = 2,
                Name = "Recipe2",
                RecipeIngredients = [ingredient2]
            };

            var mealPlan = new MealPlan
            {
                Name = "Weekly Plan",
                MealPlanRecipes =
                [
                    new() { Recipe = recipe1, RecipeId = recipe1.Id },
                    new() { Recipe = recipe2, RecipeId = recipe2.Id }
                ]
            };

            var shop = new Shop
            {
                Id = 5,
                Name = "MyShop",
                DisplaySequence =
                [
                    new()
                    {
                        ShopId = 5,
                        ProductCategoryId = category.Id,
                        Value = 7
                    }
                ]
            };

            // Act
            var result = mealPlan.MakeShoppingList(shop);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Name,
                    Is.EqualTo("Shopping list details for Weekly Plan in shop MyShop"));
                Assert.That(result.ShopId, Is.EqualTo(5));
                Assert.That(result.Products, Has.Count.EqualTo(1));

                var productItem = result.Products!.Single();
                Assert.That(productItem.ProductId, Is.EqualTo(product.Id));
                Assert.That(productItem.Quantity, Is.EqualTo(150m)); // 100 + 50 (same units)
                Assert.That(productItem.DisplaySequence, Is.EqualTo(7));
                Assert.That(productItem.Collected, Is.False);

                // Navigation props should be nulled
                Assert.That(productItem.Product, Is.Null);
                Assert.That(productItem.Unit, Is.Null);
            }
        }

        [Test]
        public void MakeShoppingList_Skips_Null_Recipes_And_Ingredients()
        {
            var mealPlan = new MealPlan
            {
                Name = "Plan",
                MealPlanRecipes =
                [
                    new() { Recipe = null!, RecipeId = 0 },
                    new()
                    {
                        Recipe = new Recipe
                        {
                            Id = 1,
                            Name = "R1",
                            RecipeIngredients = new List<RecipeIngredient?>
                            {
                                null
                            }!
                        },
                        RecipeId = 1
                    }
                ]
            };

            var shop = new Shop { Id = 1, Name = "Shop1" };

            var result = mealPlan.MakeShoppingList(shop);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Products, Is.Not.Null);
                Assert.That(result.Products, Is.Empty);
            }
        }
    }
}