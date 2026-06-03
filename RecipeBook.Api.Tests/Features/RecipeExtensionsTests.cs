using RecipeBook.Api.Features;

namespace RecipeBook.Api.Tests.Features
{
    [TestFixture]
    public class RecipeExtensionsTests
    {
        [Test]
        public void MakeShoppingList_NullShop_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new RecipeBook.Data.Entities.Recipe().MakeShoppingList(null!));
        }

        [Test]
        public void MakeShoppingList_NoIngredients_ReturnsEmptyListWithNameAndShopId()
        {
            var shopId = Guid.NewGuid();
            var recipe = new RecipeBook.Data.Entities.Recipe { Id = 1, Name = "TestRecipe", RecipeIngredients = null };
            var shop = new MealPlanner.Data.Entities.Shop { Id = shopId, Name = "MyShop" };

            var list = recipe.MakeShoppingList(shop);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(list.Name, Is.EqualTo("Shopping list details for TestRecipe in shop MyShop"));
                Assert.That(list.ShopId, Is.EqualTo(shopId));
                Assert.That(list.Products, Is.Not.Null);
                Assert.That(list.Products, Is.Empty);
            }
        }

        [Test]
        public void MakeShoppingList_SingleIngredient_CreatesSingleProduct()
        {
            var category = new RecipeBook.Data.Entities.ProductCategory { Id = 100 };
            var baseUnit = new RecipeBook.Data.Entities.Unit { Id = 1, Name = "kg" };
            var product = new RecipeBook.Data.Entities.Product { Id = 5, Name = "Flour", ProductCategory = category, BaseUnit = baseUnit };
            var recipe = new RecipeBook.Data.Entities.Recipe
            {
                Name = "Cake",
                RecipeIngredients = [new RecipeBook.Data.Entities.RecipeIngredient { ProductId = 5, Product = product, Quantity = 2m, Unit = baseUnit }]
            };
            var shop = new MealPlanner.Data.Entities.Shop { DisplaySequence = [new MealPlanner.Data.Entities.ShopDisplaySequence { Value = 3, ProductCategoryId = 100 }] };

            var list = recipe.MakeShoppingList(shop);

            Assert.That(list.Products, Has.Count.EqualTo(1));
            var p = list.Products![0];
            using (Assert.EnterMultipleScope())
            {
                Assert.That(p.ProductId, Is.EqualTo(5));
                Assert.That(p.Quantity, Is.EqualTo(2m));
                Assert.That(p.DisplaySequence, Is.EqualTo(3));
            }
        }

        [Test]
        public void MakeShoppingList_DuplicateIngredient_AccumulatesQuantity()
        {
            var baseUnit = new RecipeBook.Data.Entities.Unit { Id = 1, Name = "kg" };
            var product = new RecipeBook.Data.Entities.Product { Id = 5, Name = "Flour", BaseUnit = baseUnit };
            var recipe = new RecipeBook.Data.Entities.Recipe
            {
                Name = "Cake",
                RecipeIngredients =
                [
                    new RecipeBook.Data.Entities.RecipeIngredient { ProductId = 5, Product = product, Quantity = 1m, Unit = baseUnit },
                    new RecipeBook.Data.Entities.RecipeIngredient { ProductId = 5, Product = product, Quantity = 2m, Unit = baseUnit }
                ]
            };

            var list = recipe.MakeShoppingList(new MealPlanner.Data.Entities.Shop { DisplaySequence = [new MealPlanner.Data.Entities.ShopDisplaySequence { Value = 1 }] });

            Assert.That(list.Products, Has.Count.EqualTo(1));
            Assert.That(list.Products![0].Quantity, Is.EqualTo(3m));
        }

        [Test]
        public void MakeShoppingList_IngredientWithoutProduct_Skipped()
        {
            var recipe = new RecipeBook.Data.Entities.Recipe
            {
                Name = "Cake",
                RecipeIngredients = [new RecipeBook.Data.Entities.RecipeIngredient { ProductId = 5, Product = null, Quantity = 1m, Unit = null }]
            };

            Assert.That(recipe.MakeShoppingList(new MealPlanner.Data.Entities.Shop()).Products, Is.Empty);
        }
    }
}
