namespace Common.Data.Entities.Tests
{
    [TestFixture]
    public class RecipeTests
    {
        [Test]
        public void MakeShoppingList_NullShop_Throws()
        {
            var recipe = new Recipe();

            Assert.Throws<ArgumentNullException>(() =>
                recipe.MakeShoppingList(null!));
        }

        [Test]
        public void MakeShoppingList_NoIngredients_ReturnsEmptyListWithNameAndShopId()
        {
            // Arrange
            var recipe = new Recipe
            {
                Id = 1,
                Name = "TestRecipe",
                RecipeIngredients = null
            };

            var shop = new Shop { Id = 10, Name = "MyShop" };

            // Act
            var list = recipe.MakeShoppingList(shop);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(list.Name, Is.EqualTo("Shopping list details for TestRecipe in shop MyShop"));
                Assert.That(list.ShopId, Is.EqualTo(10));
                Assert.That(list.Products, Is.Not.Null);
                Assert.That(list.Products, Is.Empty);
            });
        }

        [Test]
        public void MakeShoppingList_SingleIngredient_CreatesSingleProduct_WithDisplaySequenceFromShop()
        {
            // Arrange
            var category = new ProductCategory { Id = 100, Name = "Cat" };
            var baseUnit = new Unit { Id = 1, Name = "kg" };
            var product = new Product
            {
                Id = 5,
                Name = "Flour",
                ProductCategory = category,
                BaseUnit = baseUnit
            };

            var ingredient = new RecipeIngredient
            {
                ProductId = 5,
                Product = product,
                Quantity = 2m,
                Unit = baseUnit
            };

            var recipe = new Recipe
            {
                Name = "Cake",
                RecipeIngredients = [ingredient]
            };

            var shop = new Shop() { DisplaySequence = [new ShopDisplaySequence() { Value = 3, ProductCategoryId = 100 }] };

            // Act
            var list = recipe.MakeShoppingList(shop);

            // Assert
            Assert.That(list.Products, Has.Count.EqualTo(1));
            var p = list.Products[0];
            Assert.Multiple(() =>
            {
                Assert.That(p.ProductId, Is.EqualTo(5));
                Assert.That(p.Quantity, Is.EqualTo(2m));
                Assert.That(p.DisplaySequence, Is.EqualTo(3));
            });
        }

        [Test]
        public void MakeShoppingList_DuplicateIngredient_SameUnit_AccumulatesQuantity()
        {
            // Arrange
            var baseUnit = new Unit { Id = 1, Name = "kg" };
            var product = new Product
            {
                Id = 5,
                Name = "Flour",
                BaseUnit = baseUnit
            };

            var ing1 = new RecipeIngredient
            {
                ProductId = 5,
                Product = product,
                Quantity = 1m,
                Unit = baseUnit
            };
            var ing2 = new RecipeIngredient
            {
                ProductId = 5,
                Product = product,
                Quantity = 2m,
                Unit = baseUnit
            };

            var recipe = new Recipe
            {
                Name = "Cake",
                RecipeIngredients = [ing1, ing2]
            };

            var shop = new Shop() { DisplaySequence = [new ShopDisplaySequence() { Value = 1 }] };

            // Act
            var list = recipe.MakeShoppingList(shop);

            // Assert
            Assert.That(list.Products, Has.Count.EqualTo(1));
            Assert.That(list.Products[0].Quantity, Is.EqualTo(3m)); // 1 + 2
        }

        [Test]
        public void MakeShoppingList_IngredientWithoutProduct_Skipped()
        {
            // Arrange
            var ingredient = new RecipeIngredient
            {
                ProductId = 5,
                Product = null,
                Quantity = 1m,
                Unit = null
            };

            var recipe = new Recipe
            {
                Name = "Cake",
                RecipeIngredients = [ingredient]
            };

            var shop = new Shop() { DisplaySequence = [new ShopDisplaySequence() { Value = 1 }] };

            // Act
            var list = recipe.MakeShoppingList(shop);

            // Assert
            Assert.That(list.Products, Is.Empty);
        }
    }
}
