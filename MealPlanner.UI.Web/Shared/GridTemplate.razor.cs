using System.Text.Json;
using BlazorBootstrap;
using Common.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MealPlanner.UI.Web.Shared
{
    public partial class GridTemplate<TItem> where TItem : BaseModel
    {
        [Parameter]
        public GridDataProviderDelegate<TItem>? DataProvider { get; set; }

        [Parameter]
        public RenderFragment? Columns { get; set; }

        [Parameter]
        public string TableGridClass { get; set; } = "table-grid";

        [Parameter]
        public string HeaderRowCssClass { get; set; } = "bg-primary text-white";

        [Parameter]
        public bool AllowPaging { get; set; } = true;

        [Inject]
        public IJSRuntime JS { get; set; } = default!;

        [Parameter]
        public EventCallback<TItem> SelectedItemChanged { get; set; }

        [Parameter]
        public TItem? SelectedItem { get; set; }

        private Grid<TItem>? gridTemplateReference;

        public async Task RefreshDataAsync()
        {
            if (gridTemplateReference != null)
            {
                await gridTemplateReference.RefreshDataAsync();
            }
        }

        public async Task OnItemSelectedAsync(GridRowEventArgs<TItem> args)
        {
            SelectedItem = args.Item;
            gridTemplateReference?.Data?.ToList().ForEach(i => i.IsSelected = false);
            args.Item.IsSelected = true;
            await SelectedItemChanged.InvokeAsync(args.Item);
        }

        private async Task OnGridSettingsChanged(GridSettings settings)
        {
            if (settings is null)
                return;

            await JS.InvokeVoidAsync("window.localStorage.setItem", typeof(TItem).ToString(), JsonSerializer.Serialize(settings));
        }

        private async Task<GridSettings> GridSettingsProvider()
        {
            var settingsJson = await JS.InvokeAsync<string>("window.localStorage.getItem", typeof(TItem).ToString());
            if (string.IsNullOrWhiteSpace(settingsJson))
                return null!;

            var settings = JsonSerializer.Deserialize<GridSettings>(settingsJson);
            if (settings is null)
                return null!;

            return settings;
        }
    }
}
