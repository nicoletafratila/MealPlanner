using Common.Pagination;
using RecipeBook.Api.Features.Unit.Queries.Search;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.Unit.Queries.Search
{
    [TestFixture]
    public class SearchQueryTests
    {
        [Test]
        public void Can_Create_SearchQuery_With_QueryParameters()
        {
            var qp = new QueryParameters<UnitModel>();
            var query = new SearchQuery { QueryParameters = qp };

            Assert.That(query.QueryParameters, Is.SameAs(qp));
        }
    }
}
