using BlazorBootstrap;
using Blazored.SessionStorage;
using Common.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace MealPlanner.UI.Web.Shared
{
    public partial class GridTemplate<TItem> where TItem : BaseModel
    {
        [Parameter]
        public GridDataProviderDelegate<TItem>? DataProvider { get; set; }

        [Parameter]
        public RenderFragment? Columns { get; set; }

        [Parameter]
        public string TableGridClass { get; set; } = "table";

        [Parameter]
        public string HeaderRowCssClass { get; set; } = "bg-primary text-white";

        [Parameter]
        public bool AllowPaging { get; set; } = true;

        [Inject]
        public ISessionStorageService? SessionStorage { get; set; }

        private Grid<TItem>? gridTemplateReference;

        public async Task RefreshDataAsync()
        {
            if (gridTemplateReference != null)
            {
                await gridTemplateReference.RefreshDataAsync();
            }
        }

        private async Task OnGridSettingsChangedAsync(GridSettings settings)
        {
            if (settings is null)
                return;

            await SessionStorage!.SetItemAsync(typeof(TItem).ToString(), JsonConvert.SerializeObject(settings));
        }

        private async Task<GridSettings> GridSettingsProviderAsync()
        {
            var settingsJson = await SessionStorage!.GetItemAsync<string>(typeof(TItem).ToString());
            if (string.IsNullOrWhiteSpace(settingsJson))
                return null!;

            var settings = JsonConvert.DeserializeObject<GridSettings>(settingsJson);
            if (settings is null)
                return null!;

            return settings;
        }
    }
}
