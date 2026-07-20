using RecipeBook.Api.Features;

namespace RecipeBook.Api.Tests.Features
{
    [TestFixture]
    public class RecipeExtensionsTests
    {
        [Test]
        public void MakeShoppingList_NullShop_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new Data.Entities.Recipe().MakeShoppingList(null!));
        }

        [Test]
        public void MakeShoppingList_NoIngredients_ReturnsEmptyListWithNameAndShopId()
        {
            var shopId = Guid.NewGuid();
            var recipe = new Data.Entities.Recipe { Id = Guid.NewGuid(), Name = "TestRecipe", RecipeIngredients = null };
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
            var categoryId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var category = new Data.Entities.ProductCategory { Id = categoryId };
            var baseUnit = new Data.Entities.Unit { Id = Guid.NewGuid(), Name = "kg" };
            var product = new Data.Entities.Product { Id = productId, Name = "Flour", ProductCategory = category, BaseUnit = baseUnit };
            var recipe = new Data.Entities.Recipe
            {
                Name = "Cake",
                RecipeIngredients = [new Data.Entities.RecipeIngredient { ProductId = productId, Product = product, Quantity = 2m, Unit = baseUnit }]
            };
            var shop = new MealPlanner.Data.Entities.Shop { DisplaySequence = [new MealPlanner.Data.Entities.ShopDisplaySequence { Value = 3, ProductCategoryId = categoryId }] };

            var list = recipe.MakeShoppingList(shop);

            Assert.That(list.Products, Has.Count.EqualTo(1));
            var p = list.Products![0];
            using (Assert.EnterMultipleScope())
            {
                Assert.That(p.ProductId, Is.EqualTo(productId));
                Assert.That(p.Quantity, Is.EqualTo(2m));
                Assert.That(p.DisplaySequence, Is.EqualTo(3));
            }
        }

        [Test]
        public void MakeShoppingList_DuplicateIngredient_AccumulatesQuantity()
        {
            var productId = Guid.NewGuid();
            var baseUnit = new Data.Entities.Unit { Id = Guid.NewGuid(), Name = "kg" };
            var product = new Data.Entities.Product { Id = productId, Name = "Flour", BaseUnit = baseUnit };
            var recipe = new Data.Entities.Recipe
            {
                Name = "Cake",
                RecipeIngredients =
                [
                    new Data.Entities.RecipeIngredient { ProductId = productId, Product = product, Quantity = 1m, Unit = baseUnit },
                    new Data.Entities.RecipeIngredient { ProductId = productId, Product = product, Quantity = 2m, Unit = baseUnit }
                ]
            };

            var list = recipe.MakeShoppingList(new MealPlanner.Data.Entities.Shop { DisplaySequence = [new MealPlanner.Data.Entities.ShopDisplaySequence { Value = 1 }] });

            Assert.That(list.Products, Has.Count.EqualTo(1));
            Assert.That(list.Products![0].Quantity, Is.EqualTo(3m));
        }

        [Test]
        public void MakeShoppingList_IngredientWithoutProduct_Skipped()
        {
            var recipe = new Data.Entities.Recipe
            {
                Name = "Cake",
                RecipeIngredients = [new Data.Entities.RecipeIngredient { ProductId = Guid.NewGuid(), Product = null, Quantity = 1m, Unit = null }]
            };

            Assert.That(recipe.MakeShoppingList(new MealPlanner.Data.Entities.Shop()).Products, Is.Empty);
        }
    }
}
