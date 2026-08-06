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

            Assert.That(SearchCacheKeyBuilder.Build("units", a), Is.EqualTo(SearchCacheKeyBuilder.Build("units", b)));
        }

        [Test]
        public void Build_DifferentPrefix_ProducesDifferentKey()
        {
            var qp = new QueryParameters<object> { PageNumber = 1, PageSize = 20 };

            Assert.That(SearchCacheKeyBuilder.Build("units", qp), Is.Not.EqualTo(SearchCacheKeyBuilder.Build("products", qp)));
        }

        [Test]
        public void Build_DifferentPageSize_ProducesDifferentKey()
        {
            var a = new QueryParameters<object> { PageNumber = 1, PageSize = 20 };
            var b = new QueryParameters<object> { PageNumber = 1, PageSize = 100 };

            Assert.That(SearchCacheKeyBuilder.Build("units", a), Is.Not.EqualTo(SearchCacheKeyBuilder.Build("units", b)));
        }

        [Test]
        public void Build_DifferentFilters_ProducesDifferentKey()
        {
            var a = new QueryParameters<object> { Filters = [new FilterItem("Name", "kg", FilterOperator.Contains)] };
            var b = new QueryParameters<object> { Filters = [new FilterItem("Name", "liter", FilterOperator.Contains)] };

            Assert.That(SearchCacheKeyBuilder.Build("units", a), Is.Not.EqualTo(SearchCacheKeyBuilder.Build("units", b)));
        }

        [Test]
        public void Build_NoFiltersVsEmptyFilters_ProducesDifferentKey()
        {
            var noFilters = new QueryParameters<object>();
            var emptyFilters = new QueryParameters<object> { Filters = [] };

            Assert.That(SearchCacheKeyBuilder.Build("units", noFilters), Is.Not.EqualTo(SearchCacheKeyBuilder.Build("units", emptyFilters)));
        }

        [Test]
        public void Build_DifferentSortDirection_ProducesDifferentKey()
        {
            var ascending = new QueryParameters<object> { Sorting = [new SortingModel { PropertyName = "Name", Direction = SortDirection.Ascending }] };
            var descending = new QueryParameters<object> { Sorting = [new SortingModel { PropertyName = "Name", Direction = SortDirection.Descending }] };

            Assert.That(SearchCacheKeyBuilder.Build("units", ascending), Is.Not.EqualTo(SearchCacheKeyBuilder.Build("units", descending)));
        }

        [Test]
        public void Build_NullQueryParameters_DoesNotThrow()
        {
            Assert.That(() => SearchCacheKeyBuilder.Build<object>("units", null), Throws.Nothing);
        }
    }
}
