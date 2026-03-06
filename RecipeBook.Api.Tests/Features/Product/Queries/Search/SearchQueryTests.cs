using Common.Pagination;
using RecipeBook.Api.Features.Product.Queries.Search;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.Product.Queries.Search
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
            using (Assert.EnterMultipleScope())
            {
                Assert.That(query.CategoryId, Is.Null);
                Assert.That(query.QueryParameters, Is.Null);
            }
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange
            var qp = new QueryParameters<ProductModel>
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
            var qp = new QueryParameters<ProductModel>
            {
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            query.CategoryId = "3";
            query.QueryParameters = qp;

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(query.CategoryId, Is.EqualTo("3"));
                Assert.That(query.QueryParameters, Is.SameAs(qp));
            }
        }
    }
}