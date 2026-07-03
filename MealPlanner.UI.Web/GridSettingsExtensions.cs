using BlazorBootstrap;
using Common.Pagination;

namespace MealPlanner.UI.Web
{
    public static class GridSettingsExtensions
    {
        public static QueryParameters<T> ToQueryParameters<T>(this GridDataProviderRequest<T> request)
        {
            return new QueryParameters<T>
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Filters = request.Filters?
                    .Select(f => new Common.Pagination.FilterItem(
                        f.PropertyName,
                        f.Value,
                        (Common.Pagination.FilterOperator)(int)f.Operator,
                        f.StringComparison))
                    .ToList(),
                Sorting = request.Sorting?
                    .Select(s => new SortingModel
                    {
                        PropertyName = s.SortString,
                        Direction = (Common.Pagination.SortDirection)(int)s.SortDirection
                    })
                    .ToList() ?? []
            };
        }

        public static GridSettings ToGridSettings<T>(this QueryParameters<T> qp) =>
            new() { PageNumber = qp.PageNumber, PageSize = qp.PageSize };
    }
}
