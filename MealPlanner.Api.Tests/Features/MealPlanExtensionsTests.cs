using MealPlanner.Api.Features;

namespace MealPlanner.Api.Tests.Features
{
    [TestFixture]
    public class MealPlanExtensionsTests
    {
        [Test]
        public void MakeShoppingList_Returns_Empty_When_Shop_Is_Null()
        {
            var result = new MealPlanner.Data.Entities.MealPlan { Name = "Plan" }.MakeShoppingList(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.ShopId, Is.EqualTo(Guid.Empty));
                Assert.That(result.Name, Is.Null);
            }
        }

        [Test]
        public void MakeShoppingList_Returns_Empty_Products_When_No_Recipes()
        {
            var shopId = Guid.NewGuid();
            var result = new MealPlanner.Data.Entities.MealPlan { Name = "Plan" }
                .MakeShoppingList(new MealPlanner.Data.Entities.Shop { Id = shopId, Name = "Shop1" });

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Name, Is.EqualTo("Shopping list details for Plan in shop Shop1"));
                Assert.That(result.ShopId, Is.EqualTo(shopId));
                Assert.That(result.Products, Is.Empty);
            }
        }

        [Test]
        public void MakeShoppingList_Aggregates_Quantities_Per_Product()
        {
            var baseUnit = new RecipeBook.Data.Entities.Unit { Id = 2, Name = "gr" };
            var category = new RecipeBook.Data.Entities.ProductCategory { Id = Guid.NewGuid() };
            var product = new RecipeBook.Data.Entities.Product
            {
                Id = 10, Name = "Flour",
                BaseUnit = baseUnit, BaseUnitId = baseUnit.Id,
                ProductCategory = category, ProductCategoryId = category.Id
            };

            var mealPlan = new MealPlanner.Data.Entities.MealPlan
            {
                Name = "Weekly Plan",
                MealPlanRecipes =
                [
                    new MealPlanner.Data.Entities.MealPlanRecipe { RecipeId = 1, Recipe = new RecipeBook.Data.Entities.Recipe { Id = 1, RecipeIngredients = [new RecipeBook.Data.Entities.RecipeIngredient { Product = product, ProductId = product.Id, Quantity = 100m, Unit = baseUnit, UnitId = baseUnit.Id }] } },
                    new MealPlanner.Data.Entities.MealPlanRecipe { RecipeId = 2, Recipe = new RecipeBook.Data.Entities.Recipe { Id = 2, RecipeIngredients = [new RecipeBook.Data.Entities.RecipeIngredient { Product = product, ProductId = product.Id, Quantity = 50m, Unit = baseUnit, UnitId = baseUnit.Id }] } }
                ]
            };

            var shopId = Guid.NewGuid();
            var shop = new MealPlanner.Data.Entities.Shop { Id = shopId, Name = "MyShop", DisplaySequence = [new MealPlanner.Data.Entities.ShopDisplaySequence { ShopId = shopId, ProductCategoryId = category.Id, Value = 7 }] };

            var result = mealPlan.MakeShoppingList(shop);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.ShopId, Is.EqualTo(shopId));
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
            var mealPlan = new MealPlanner.Data.Entities.MealPlan
            {
                Name = "Plan",
                MealPlanRecipes =
                [
                    new MealPlanner.Data.Entities.MealPlanRecipe { Recipe = null!, RecipeId = 0 },
                    new MealPlanner.Data.Entities.MealPlanRecipe { RecipeId = 1, Recipe = new RecipeBook.Data.Entities.Recipe { Id = 1, RecipeIngredients = new List<RecipeBook.Data.Entities.RecipeIngredient?> { null }! } }
                ]
            };

            var result = mealPlan.MakeShoppingList(new MealPlanner.Data.Entities.Shop { Id = Guid.NewGuid(), Name = "Shop1" });

            Assert.That(result.Products, Is.Empty);
        }
    }
}
