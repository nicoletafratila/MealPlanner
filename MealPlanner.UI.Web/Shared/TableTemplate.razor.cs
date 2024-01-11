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
        public IEnumerable<TItem>? Data { get; set; }
    }
}
