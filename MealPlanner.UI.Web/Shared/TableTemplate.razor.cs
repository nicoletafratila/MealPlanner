using Common.Shared;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Shared
{
    public partial class TableTemplate<TItem> where TItem : BaseModel
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

        [Parameter]
        public EventCallback<TItem> SelectedItemChanged { get; set; }

        [Parameter]
        public TItem? SelectedItem { get; set; }

        public async void OnSelectedItemChanged(TItem item)
        {
            SelectedItem = item;
            Data?.ToList().ForEach(i => i.IsSelected = false);
            item.IsSelected = true;
            await SelectedItemChanged.InvokeAsync(item);
        }
    }
}
