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

        [Inject]
        public IJSRuntime JS { get; set; } = default!;

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
