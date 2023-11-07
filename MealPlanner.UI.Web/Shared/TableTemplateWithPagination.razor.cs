using Common.Pagination;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Shared
{
    public partial class TableTemplateWithPagination<TItem>
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
        public PagedList<TItem>? Data { get; set; }

        private int CalculateIndex(int indexFromView)
        {
            return Data!.Metadata!.PageSize * (Data!.Metadata!.PageNumber - 1) + indexFromView;
        }
    }
}
