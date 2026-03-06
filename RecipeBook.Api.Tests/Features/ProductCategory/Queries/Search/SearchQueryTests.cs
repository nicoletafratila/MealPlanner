using Common.Pagination;
using RecipeBook.Api.Features.ProductCategory.Queries.Search;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.ProductCategory.Queries.Search
{
    [TestFixture]
    public class SearchQueryTests
    {
        [Test]
        public void DefaultCtor_InitializesQueryParametersToNull()
        {
            // Act
            var query = new SearchQuery();

            // Assert
            Assert.That(query.QueryParameters, Is.Null);
        }

        [Test]
        public void Ctor_SetsQueryParameters()
        {
            // Arrange
            var qp = new QueryParameters<ProductCategoryModel>
            {
                PageNumber = 2,
                PageSize = 20
            };

            // Act
            var query = new SearchQuery(qp);

            // Assert
            Assert.That(query.QueryParameters, Is.SameAs(qp));
        }

        [Test]
        public void Can_Set_And_Get_QueryParameters_Property()
        {
            // Arrange
            var query = new SearchQuery();
            var qp = new QueryParameters<ProductCategoryModel>
            {
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            query.QueryParameters = qp;

            // Assert
            Assert.That(query.QueryParameters, Is.SameAs(qp));
        }
    }
}