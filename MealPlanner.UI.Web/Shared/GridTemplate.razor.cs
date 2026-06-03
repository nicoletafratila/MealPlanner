using BlazorBootstrap;
using Common.Models;
using Common.UI;
using MealPlanner.UI.Web.Shared.Resources;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Shared
{
    public partial class GridTemplate<TItem> where TItem : BaseModel
    {
        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Parameter]
        public GridDataProviderDelegate<TItem>? DataProvider { get; set; }

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
        public int PageSize { get; set; } = 10;

        [Parameter]
        public int[] PageSizeSelectorItems { get; set; } = [5, 10, 20, 50];

        [Parameter]
        public GridSettingsProviderDelegate? SettingsProvider { get; set; }

        private Grid<TItem>? gridTemplateReference;

        private async Task<GridDataProviderResult<TItem>> DataProviderWrapperAsync(GridDataProviderRequest<TItem> request)
        {
            if (DataProvider is null)
                return new GridDataProviderResult<TItem> { Data = [], TotalCount = 0 };

            try
            {
                return await DataProvider(request);
            }
            catch (HttpRequestException)
            {
                if (MessageComponent is not null)
                    await MessageComponent.ShowErrorAsync(Messages.ServiceUnavailable);

                return new GridDataProviderResult<TItem> { Data = [], TotalCount = 0 };
            }
        }

        private async Task<GridSettings?> SettingsProviderWrapperAsync()
        {
            var settings = SettingsProvider != null ? await SettingsProvider() : null;
            if (settings is { PageSize: > 0 } && PageSizeSelectorItems.Contains(settings.PageSize))
                return settings;
            return new GridSettings { PageNumber = 1, PageSize = PageSize };
        }

        public async Task RefreshDataAsync()
        {
            if (gridTemplateReference != null)
            {
                await gridTemplateReference.RefreshDataAsync();
            }
        }
    }
}
