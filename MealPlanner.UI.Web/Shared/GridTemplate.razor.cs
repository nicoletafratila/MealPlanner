using BlazorBootstrap;
using Common.Models;
using Common.Pagination;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Shared
{
    public partial class GridTemplate<TItem> where TItem : BaseModel
    {
        [Parameter]
        public GridDataProviderDelegate<TItem>? DataProvider { get; set; }

        [Parameter]
        public GridQueryParametersProviderDelegate<TItem>? QueryParametersProvider { get; set; }

        [Parameter]
        public RenderFragment? Columns { get; set; }

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        [Parameter]
        public string TableGridClass { get; set; } = "table";

        [Parameter]
        public string HeaderRowCssClass { get; set; } = "bg-primary text-white";

        [Parameter]
        public bool AllowPaging { get; set; } = true;

        [Parameter]
        public GridSettingsProviderDelegate? SettingsProvider
        {
            get => _settingsProvider ?? GetSettingsFromQueryParametersAsync;
            set => _settingsProvider = value;
        }
        private GridSettingsProviderDelegate? _settingsProvider;

        private Grid<TItem>? gridTemplateReference;

        public async Task RefreshDataAsync()
        {
            if (gridTemplateReference != null)
            {
                await gridTemplateReference.RefreshDataAsync();
            }
        }

        private async Task<GridSettings> GetSettingsFromQueryParametersAsync()
        {
            if (QueryParametersProvider is null)
                return null!;

            var gridParams = await QueryParametersProvider();

            if (gridParams is null)
                return null!;

            StateHasChanged();

            return new GridSettings
            {
                PageNumber = gridParams.PageNumber,
                PageSize = gridParams.PageSize,
                Filters = gridParams.Filters
            };
        }
    }


    public delegate Task<QueryParameters<TItem>> GridQueryParametersProviderDelegate<TItem>();
}
