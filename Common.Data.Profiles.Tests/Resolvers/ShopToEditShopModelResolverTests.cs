using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using MealPlanner.Shared.Models;
using Moq;

namespace Common.Data.Profiles.Tests.Resolvers
{
    [TestFixture]
    public class ShopToEditShopModelResolverTests
    {
        [Test]
        public void Resolve_Maps_Orders_And_Sets_Indexes()
        {
            // Arrange
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);

            var sourceSequences = new List<ShopDisplaySequence>
            {
                new() { Value = 20 },
                new() { Value = 10 }
            };

            var mapped1 = new ShopDisplaySequenceEditModel { Value = 20 };
            var mapped2 = new ShopDisplaySequenceEditModel { Value = 10 };

            mapperMock.Setup(m => m.Map<ShopDisplaySequenceEditModel>(sourceSequences[0]))
                      .Returns(mapped1);
            mapperMock.Setup(m => m.Map<ShopDisplaySequenceEditModel>(sourceSequences[1]))
                      .Returns(mapped2);

            var resolver = new ShopToEditShopModelResolver(mapperMock.Object);

            var context = default(ResolutionContext);

            // Act
            var result = resolver.Resolve(
                source: new Shop(),
                destination: new ShopEditModel(),
                sourceValue: sourceSequences,
                destValue: null,
                context: context);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Has.Count.EqualTo(2));

                // Ordered by Value ascending
                Assert.That(result[0].Value, Is.EqualTo(10));
                Assert.That(result[1].Value, Is.EqualTo(20));

                // Indexes set by SetIndexes() (1-based)
                Assert.That(result[0].Index, Is.EqualTo(1));
                Assert.That(result[1].Index, Is.EqualTo(2));
            }

            mapperMock.VerifyAll();
        }

        [Test]
        public void Resolve_Returns_Empty_When_SourceValue_Null_Or_Empty()
        {
            var mapperMock = new Mock<IMapper>();
            var resolver = new ShopToEditShopModelResolver(mapperMock.Object);

            var empty = resolver.Resolve(new Shop(), new ShopEditModel(), null, null, default);
            Assert.That(empty, Is.Empty);

            var emptyList = resolver.Resolve(new Shop(), new ShopEditModel(), [], null, default);
            Assert.That(emptyList, Is.Empty);

            mapperMock.VerifyNoOtherCalls();
        }
    }
}