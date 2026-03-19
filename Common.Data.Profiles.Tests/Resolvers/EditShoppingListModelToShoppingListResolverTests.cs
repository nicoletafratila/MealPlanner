using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using MealPlanner.Shared.Models;
using Moq;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests.Resolvers
{
    [TestFixture]
    public class EditShoppingListModelToShoppingListResolverTests
    {
        [Test]
        public void Resolve_Maps_All_Items()
        {
            // Arrange
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);

            var sourceProducts = new List<ShoppingListProductEditModel>
            {
                new()
                {
                    Collected = false,
                    DisplaySequence = 1,
                    Product = new ProductModel { Name = "Apples" }
                },
                new()
                {
                    Collected = true,
                    DisplaySequence = 2,
                    Product = new ProductModel { Name = "Bananas" }
                }
            };

            var mapped1 = new ShoppingListProduct
            {
                Collected = false,
                DisplaySequence = 1,
                Product = new Product { Name = "Apples" }
            };

            var mapped2 = new ShoppingListProduct
            {
                Collected = true,
                DisplaySequence = 2,
                Product = new Product { Name = "Bananas" }
            };

            mapperMock.Setup(m => m.Map<ShoppingListProduct>(sourceProducts[0]))
                      .Returns(mapped1);
            mapperMock.Setup(m => m.Map<ShoppingListProduct>(sourceProducts[1]))
                      .Returns(mapped2);

            var resolver = new EditShoppingListModelToShoppingListResolver(mapperMock.Object);
            var context = default(ResolutionContext);

            var editModel = new ShoppingListEditModel
            {
                Products = sourceProducts
            };

            // Act
            var result = resolver.Resolve(
                source: editModel,
                destination: new ShoppingList(),
                sourceValue: sourceProducts,
                destValue: null,
                context: context);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Has.Count.EqualTo(2));

                Assert.That(result![0].Product!.Name, Is.EqualTo("Apples"));
                Assert.That(result[1].Product!.Name, Is.EqualTo("Bananas"));
            }

            mapperMock.VerifyAll();
        }

        [Test]
        public void Resolve_Returns_Empty_When_SourceProducts_Null_Or_Empty()
        {
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            var resolver = new EditShoppingListModelToShoppingListResolver(mapperMock.Object);
            var context = default(ResolutionContext);

            var empty1 = resolver.Resolve(
                new ShoppingListEditModel { Products = null },
                new ShoppingList(),
                null,
                null,
                context);

            var empty2 = resolver.Resolve(
                new ShoppingListEditModel { Products = [] },
                new ShoppingList(),
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