using Common.Pagination;

namespace Common.Services.Tests
{
    [TestFixture]
    public class SearchCacheKeyBuilderTests
    {
        [Test]
        public void Build_SameParameters_ProducesSameKey()
        {
            var a = new QueryParameters<object> { PageNumber = 1, PageSize = 20 };
            var b = new QueryParameters<object> { PageNumber = 1, PageSize = 20 };

            Assert.That(SearchCacheKeyBuilder.Build("units", a, "user-1"), Is.EqualTo(SearchCacheKeyBuilder.Build("units", b, "user-1")));
        }

        [Test]
        public void Build_DifferentPrefix_ProducesDifferentKey()
        {
            var qp = new QueryParameters<object> { PageNumber = 1, PageSize = 20 };

            Assert.That(SearchCacheKeyBuilder.Build("units", qp, "user-1"), Is.Not.EqualTo(SearchCacheKeyBuilder.Build("products", qp, "user-1")));
        }

        [Test]
        public void Build_DifferentPageSize_ProducesDifferentKey()
        {
            var a = new QueryParameters<object> { PageNumber = 1, PageSize = 20 };
            var b = new QueryParameters<object> { PageNumber = 1, PageSize = 100 };

            Assert.That(SearchCacheKeyBuilder.Build("units", a, "user-1"), Is.Not.EqualTo(SearchCacheKeyBuilder.Build("units", b, "user-1")));
        }

        [Test]
        public void Build_DifferentFilters_ProducesDifferentKey()
        {
            var a = new QueryParameters<object> { Filters = [new FilterItem("Name", "kg", FilterOperator.Contains)] };
            var b = new QueryParameters<object> { Filters = [new FilterItem("Name", "liter", FilterOperator.Contains)] };

            Assert.That(SearchCacheKeyBuilder.Build("units", a, "user-1"), Is.Not.EqualTo(SearchCacheKeyBuilder.Build("units", b, "user-1")));
        }

        [Test]
        public void Build_NoFiltersVsEmptyFilters_ProducesDifferentKey()
        {
            var noFilters = new QueryParameters<object>();
            var emptyFilters = new QueryParameters<object> { Filters = [] };

            Assert.That(SearchCacheKeyBuilder.Build("units", noFilters, "user-1"), Is.Not.EqualTo(SearchCacheKeyBuilder.Build("units", emptyFilters, "user-1")));
        }

        [Test]
        public void Build_DifferentSortDirection_ProducesDifferentKey()
        {
            var ascending = new QueryParameters<object> { Sorting = [new SortingModel { PropertyName = "Name", Direction = SortDirection.Ascending }] };
            var descending = new QueryParameters<object> { Sorting = [new SortingModel { PropertyName = "Name", Direction = SortDirection.Descending }] };

            Assert.That(SearchCacheKeyBuilder.Build("units", ascending, "user-1"), Is.Not.EqualTo(SearchCacheKeyBuilder.Build("units", descending, "user-1")));
        }

        [Test]
        public void Build_NullQueryParameters_DoesNotThrow()
        {
            Assert.That(() => SearchCacheKeyBuilder.Build<object>("units", null, "user-1"), Throws.Nothing);
        }

        [Test]
        public void Build_DifferentUserId_ProducesDifferentKey()
        {
            var qp = new QueryParameters<object> { PageNumber = 1, PageSize = 20 };

            Assert.That(SearchCacheKeyBuilder.Build("shops", qp, "user-1"), Is.Not.EqualTo(SearchCacheKeyBuilder.Build("shops", qp, "user-2")));
        }

        [Test]
        public void Build_NullUserId_DoesNotThrow()
        {
            var qp = new QueryParameters<object> { PageNumber = 1, PageSize = 20 };

            Assert.That(() => SearchCacheKeyBuilder.Build("units", qp, null), Throws.Nothing);
        }
    }
}
