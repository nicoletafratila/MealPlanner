using Common.Pagination;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Shared
{
    public partial class TableTemplate<TItem>
    {
        [Parameter]
        public bool ShowIndex { get; set; } = false;

        [Parameter]
        public RenderFragment? TableHeader { get; set; }

        [Parameter]
        public RenderFragment? TableCaption { get; set; }

        [Parameter]
        public RenderFragment<TItem>? RowTemplate { get; set; }

        [Parameter]
        public IEnumerable<TItem>? Items { get; set; }

        //private int CalculateIndex(int indexFromView)
        //{
        //    var pageNumber = 1;
        //    var pageSize = int.MaxValue;
        //    PagedList<TItem>? list = Items as PagedList<TItem>;
        //    if (list != null)
        //    {
        //        pageNumber = list.Metadata!.PageNumber;
        //        pageSize = list.Metadata!.PageSize;
        //    }

        //    return pageSize * (pageNumber - 1) + indexFromView + 1;
        //}
    }
}
