using Common.Pagination;
using MealPlanner.Api.Features.MealPlan.Queries.Search;
using MealPlanner.Shared.Models;

namespace MealPlanner.Api.Tests.Features.MealPlan.Queries.Search
{
    [TestFixture]
    public class SearchQueryTests
    {
        [Test]
        public void DefaultCtor_InitializesQueryParametersToNull()
        {
            var query = new SearchQuery();
            Assert.That(query.QueryParameters, Is.Null);
        }

        [Test]
        public void Ctor_SetsQueryParameters()
        {
            var qp = new QueryParameters<MealPlanModel>
            {
                PageNumber = 2,
                PageSize = 20
            };

            var query = new SearchQuery(qp);

            Assert.That(query.QueryParameters, Is.SameAs(qp));
        }

        [Test]
        public void Can_Set_And_Get_QueryParameters()
        {
            var query = new SearchQuery();
            var qp = new QueryParameters<MealPlanModel>
            {
                PageNumber = 1,
                PageSize = 10
            };

            query.QueryParameters = qp;

            Assert.That(query.QueryParameters, Is.SameAs(qp));
        }
    }
}