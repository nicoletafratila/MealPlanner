using Common.Pagination;
using MealPlanner.Api.Features.ShoppingList.Queries.Search;
using MealPlanner.Shared.Models;

namespace MealPlanner.Api.Tests.Features.ShoppingList.Queries.Search
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
            var qp = new QueryParameters<ShoppingListModel>
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
        public void Can_Set_And_Get_QueryParameters()
        {
            // Arrange
            var query = new SearchQuery();
            var qp = new QueryParameters<ShoppingListModel>
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