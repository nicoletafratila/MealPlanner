using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using MealPlanner.Shared.Models;
using Moq;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests.Resolvers
{
    [TestFixture]
    public class ShoppingListToEditShoppingListModelResolverTests
    {
        [Test]
        public void Resolve_Maps_And_Orders_Correctly()
        {
            // Arrange
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);

            var sourceProducts = new List<ShoppingListProduct>
            {
                new()
                {
                    Collected = true,
                    DisplaySequence = 5,
                    Product = new Product { Name = "Bananas" }
                },
                new()
                {
                    Collected = false,
                    DisplaySequence = 2,
                    Product = new Product { Name = "Apples" }
                },
                new()
                {
                    Collected = false,
                    DisplaySequence = 2,
                    Product = new Product { Name = "Avocado" }
                }
            };

            var mapped1 = new ShoppingListProductEditModel
            {
                Collected = true,
                DisplaySequence = 5,
                Product = new ProductModel { Name = "Bananas" }
            };

            var mapped2 = new ShoppingListProductEditModel
            {
                Collected = false,
                DisplaySequence = 2,
                Product = new ProductModel { Name = "Apples" }
            };

            var mapped3 = new ShoppingListProductEditModel
            {
                Collected = false,
                DisplaySequence = 2,
                Product = new ProductModel { Name = "Avocado" }
            };

            mapperMock.Setup(m => m.Map<ShoppingListProductEditModel>(sourceProducts[0]))
                      .Returns(mapped1);
            mapperMock.Setup(m => m.Map<ShoppingListProductEditModel>(sourceProducts[1]))
                      .Returns(mapped2);
            mapperMock.Setup(m => m.Map<ShoppingListProductEditModel>(sourceProducts[2]))
                      .Returns(mapped3);

            var resolver = new ShoppingListToEditShoppingListModelResolver(mapperMock.Object);
            var context = default(ResolutionContext);

            var model = new ShoppingList { Products = sourceProducts };

            // Act
            var result = resolver.Resolve(
                source: model,
                destination: new ShoppingListEditModel(),
                sourceValue: sourceProducts,
                destValue: null,
                context: context);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Has.Count.EqualTo(3));

                // Expected ordering:
                // 1. Collected = false
                // 2. DisplaySequence ascending
                // 3. Product.Name alphabetical
                Assert.That(result![0].Product!.Name, Is.EqualTo("Apples"));
                Assert.That(result[1].Product!.Name, Is.EqualTo("Avocado"));
                Assert.That(result[2].Product!.Name, Is.EqualTo("Bananas"));
            }

            mapperMock.VerifyAll();
        }

        [Test]
        public void Resolve_Returns_Empty_When_SourceValue_Null_Or_Empty()
        {
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            var resolver = new ShoppingListToEditShoppingListModelResolver(mapperMock.Object);
            var context = default(ResolutionContext);

            var empty1 = resolver.Resolve(
                new ShoppingList { Products = null },
                new ShoppingListEditModel(),
                null,
                null,
                context);

            var empty2 = resolver.Resolve(
                new ShoppingList { Products = [] },
                new ShoppingListEditModel(),
                [],
                null,
                context);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(empty1, Is.Empty);
                Assert.That(empty2, Is.Empty);
            }

            mapperMock.VerifyNoOtherCalls();
        }
    }
}