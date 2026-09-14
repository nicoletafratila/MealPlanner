using Common.Models;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Shared
{
    public partial class TableTemplate<TItem> where TItem : BaseModel
    {
        [Parameter]
        public bool ShowIndex { get; set; }

        [Parameter]
        public string IndexHeaderText { get; set; } = "#";

        [Parameter]
        public RenderFragment? TableHeader { get; set; }

        [Parameter]
        public int ColumnCount { get; set; }

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

        [Parameter]
        public bool EnableDragReorder { get; set; }

        [Parameter]
        public EventCallback<(TItem DraggedItem, TItem TargetItem)> OnReorder { get; set; }

        private TItem? _draggedItem;

        public void OnDragStart(TItem item)
        {
            _draggedItem = item;
        }

        public async Task OnDropAsync(TItem item)
        {
            var draggedItem = _draggedItem;
            _draggedItem = default;

            if (draggedItem is null || EqualityComparer<TItem>.Default.Equals(draggedItem, item))
                return;

            if (OnReorder.HasDelegate)
                await OnReorder.InvokeAsync((draggedItem, item));
        }

        public async Task OnSelectedItemChangedAsync(TItem item)
        {
            if (item is null)
                return;

            if (EqualityComparer<TItem>.Default.Equals(item, SelectedItem))
                return;

            if (Data is not null)
            {
                foreach (var i in Data)
                {
                    i.IsSelected = ReferenceEquals(i, item);
                }
            }

            SelectedItem = item;

            if (SelectedItemChanged.HasDelegate)
            {
                await SelectedItemChanged.InvokeAsync(item);
            }
        }
    }
}
