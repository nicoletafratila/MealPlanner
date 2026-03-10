using MealPlanner.Api.Features.Statistics.Queries.SearchProducts;

namespace MealPlanner.Api.Tests.Features.Statistics.Queries.SearchProducts
{
    [TestFixture]
    public class SearchQueryTests
    {
        [Test]
        public void DefaultCtor_InitializesPropertiesToNull()
        {
            // Act
            var query = new SearchQuery();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(query.CategoryIds, Is.Null);
                Assert.That(query.AuthToken, Is.Null);
            });
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange / Act
            var query = new SearchQuery("1,2,3", "token123");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(query.CategoryIds, Is.EqualTo("1,2,3"));
                Assert.That(query.AuthToken, Is.EqualTo("token123"));
            });
        }

        [Test]
        public void Can_Set_And_Get_Properties()
        {
            // Arrange
            var query = new SearchQuery
            {
                // Act
                CategoryIds = "4,5",
                AuthToken = "abc"
            };

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(query.CategoryIds, Is.EqualTo("4,5"));
                Assert.That(query.AuthToken, Is.EqualTo("abc"));
            });
        }
    }
}