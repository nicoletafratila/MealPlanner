using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ShopsOverview
    {
        //[Parameter]
        //public QueryParameters? QueryParameters { get; set; } = new();

        public EditShopModel? Shop { get; set; }
        public IList<ShopModel>? Shops { get; set; }

        [Inject]
        public IShopService? ShopService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public IJSRuntime? JSRuntime { get; set; }

        [CascadingParameter(Name = "ErrorComponent")]
        protected IErrorComponent? ErrorComponent { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await RefreshAsync();
        }

        private void New()
        {
            NavigationManager!.NavigateTo($"shopedit/");
        }

        private void Update(ShopModel item)
        {
            NavigationManager!.NavigateTo($"shopedit/{item.Id}");
        }

        private async void DeleteAsync(ShopModel item)
        {
            if (item != null)
            {
                if (!await JSRuntime!.Confirm($"Are you sure you want to delete the meal plan: '{item.Name}'?"))
                    return;

                var response = await ShopService!.DeleteAsync(item.Id);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    ErrorComponent!.ShowError("Error", response);
                }
                else
                {
                    await RefreshAsync();
                }
            }
        }

        private async Task RefreshAsync()
        {
            Shops = await ShopService!.GetAllAsync();
            StateHasChanged();
        }

        //private async void OnPageChangedAsync(int pageNumber)
        //{
        //    QueryParameters!.PageNumber = pageNumber;
        //    await RefreshAsync();
        //}
    }
}
