using MealPlanner.Api.Features;

namespace MealPlanner.Api.Tests.Features
{
    [TestFixture]
    public class MealPlanExtensionsTests
    {
        [Test]
        public void MakeShoppingList_Returns_Empty_When_Shop_Is_Null()
        {
            var result = new Common.Data.Entities.MealPlan { Name = "Plan" }.MakeShoppingList(null);

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
            var result = new Common.Data.Entities.MealPlan { Name = "Plan" }
                .MakeShoppingList(new Common.Data.Entities.Shop { Id = 1, Name = "Shop1" });

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Name, Is.EqualTo("Shopping list details for Plan in shop Shop1"));
                Assert.That(result.ShopId, Is.EqualTo(1));
                Assert.That(result.Products, Is.Empty);
            }
        }

        [Test]
        public void MakeShoppingList_Aggregates_Quantities_Per_Product()
        {
            var baseUnit = new Common.Data.Entities.Unit { Id = 2, Name = "gr" };
            var category = new Common.Data.Entities.ProductCategory { Id = 3 };
            var product = new Common.Data.Entities.Product
            {
                Id = 10, Name = "Flour",
                BaseUnit = baseUnit, BaseUnitId = baseUnit.Id,
                ProductCategory = category, ProductCategoryId = category.Id
            };

            var mealPlan = new Common.Data.Entities.MealPlan
            {
                Name = "Weekly Plan",
                MealPlanRecipes =
                [
                    new Common.Data.Entities.MealPlanRecipe { RecipeId = 1, Recipe = new Common.Data.Entities.Recipe { Id = 1, RecipeIngredients = [new Common.Data.Entities.RecipeIngredient { Product = product, ProductId = product.Id, Quantity = 100m, Unit = baseUnit, UnitId = baseUnit.Id }] } },
                    new Common.Data.Entities.MealPlanRecipe { RecipeId = 2, Recipe = new Common.Data.Entities.Recipe { Id = 2, RecipeIngredients = [new Common.Data.Entities.RecipeIngredient { Product = product, ProductId = product.Id, Quantity = 50m, Unit = baseUnit, UnitId = baseUnit.Id }] } }
                ]
            };

            var shop = new Common.Data.Entities.Shop { Id = 5, Name = "MyShop", DisplaySequence = [new Common.Data.Entities.ShopDisplaySequence { ShopId = 5, ProductCategoryId = category.Id, Value = 7 }] };

            var result = mealPlan.MakeShoppingList(shop);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.ShopId, Is.EqualTo(5));
                Assert.That(result.Products, Has.Count.EqualTo(1));
                var item = result.Products!.Single();
                Assert.That(item.Quantity, Is.EqualTo(150m));
                Assert.That(item.DisplaySequence, Is.EqualTo(7));
                Assert.That(item.Product, Is.Null);
                Assert.That(item.Unit, Is.Null);
            }
        }

        [Test]
        public void MakeShoppingList_Skips_Null_Recipes_And_Ingredients()
        {
            var mealPlan = new Common.Data.Entities.MealPlan
            {
                Name = "Plan",
                MealPlanRecipes =
                [
                    new Common.Data.Entities.MealPlanRecipe { Recipe = null!, RecipeId = 0 },
                    new Common.Data.Entities.MealPlanRecipe { RecipeId = 1, Recipe = new Common.Data.Entities.Recipe { Id = 1, RecipeIngredients = new List<Common.Data.Entities.RecipeIngredient?> { null }! } }
                ]
            };

            var result = mealPlan.MakeShoppingList(new Common.Data.Entities.Shop { Id = 1, Name = "Shop1" });

            Assert.That(result.Products, Is.Empty);
        }
    }
}
