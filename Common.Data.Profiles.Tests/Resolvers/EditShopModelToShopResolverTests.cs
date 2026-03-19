using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using MealPlanner.Shared.Models;
using Moq;

namespace Common.Data.Profiles.Tests.Resolvers
{
    [TestFixture]
    public class EditShopModelToShopResolverTests
    {
        [Test]
        public void Resolve_Maps_Orders_Correctly()
        {
            // Arrange
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);

            var sourceSequences = new List<ShopDisplaySequenceEditModel>
            {
                new() { Value = 30 },
                new() { Value = 10 },
                new() { Value = 20 }
            };

            var mapped1 = new ShopDisplaySequence { Value = 30 };
            var mapped2 = new ShopDisplaySequence { Value = 10 };
            var mapped3 = new ShopDisplaySequence { Value = 20 };

            mapperMock.Setup(m => m.Map<ShopDisplaySequence>(sourceSequences[0]))
                      .Returns(mapped1);
            mapperMock.Setup(m => m.Map<ShopDisplaySequence>(sourceSequences[1]))
                      .Returns(mapped2);
            mapperMock.Setup(m => m.Map<ShopDisplaySequence>(sourceSequences[2]))
                      .Returns(mapped3);

            var resolver = new EditShopModelToShopResolver(mapperMock.Object);
            var context = default(ResolutionContext);

            var editShop = new ShopEditModel
            {
                DisplaySequence = sourceSequences
            };

            // Act
            var result = resolver.Resolve(
                source: editShop,
                destination: new Shop(),
                sourceValue: sourceSequences,
                destValue: null,
                context: context);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Has.Count.EqualTo(3));

                // Ordered by Value ascending
                Assert.That(result![0].Value, Is.EqualTo(10));
                Assert.That(result[1].Value, Is.EqualTo(20));
                Assert.That(result[2].Value, Is.EqualTo(30));
            }

            mapperMock.VerifyAll();
        }

        [Test]
        public void Resolve_Returns_Empty_When_SourceValue_Null_Or_Empty()
        {
            // Arrange
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            var resolver = new EditShopModelToShopResolver(mapperMock.Object);
            var context = default(ResolutionContext);

            // Act
            var empty = resolver.Resolve(
                new ShopEditModel { DisplaySequence = null },
                new Shop(),
                null,
                null,
                context);

            var emptyList = resolver.Resolve(
                new ShopEditModel { DisplaySequence = [] },
                new Shop(),
                [],
                null,
                context);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(empty, Is.Empty);
                Assert.That(emptyList, Is.Empty);
            }

            mapperMock.VerifyNoOtherCalls();
        }
    }
}