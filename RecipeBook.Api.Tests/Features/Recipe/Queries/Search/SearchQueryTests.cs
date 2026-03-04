using Common.Pagination;
using RecipeBook.Api.Features.Recipe.Queries.Search;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.Recipe.Queries.Search
{
    [TestFixture]
    public class SearchQueryTests
    {
        [Test]
        public void DefaultCtor_InitializesPropertiesToNull()
        {
            // Act
            var query = new SearchQuery();

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(query.CategoryId, Is.Null);
                Assert.That(query.QueryParameters, Is.Null);
            }
        }

        [Test]
        public void Ctor_SetsCategoryId_And_QueryParameters()
        {
            // Arrange
            var qp = new QueryParameters<RecipeModel>
            {
                PageNumber = 2,
                PageSize = 20
            };

            // Act
            var query = new SearchQuery("5", qp);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(query.CategoryId, Is.EqualTo("5"));
                Assert.That(query.QueryParameters, Is.SameAs(qp));
            }
        }

        [Test]
        public void Can_Set_And_Get_Properties()
        {
            // Arrange
            var query = new SearchQuery();
            var qp = new QueryParameters<RecipeModel>
            {
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            query.CategoryId = "3";
            query.QueryParameters = qp;

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(query.CategoryId, Is.EqualTo("3"));
                Assert.That(query.QueryParameters, Is.SameAs(qp));
            }
        }
    }
}