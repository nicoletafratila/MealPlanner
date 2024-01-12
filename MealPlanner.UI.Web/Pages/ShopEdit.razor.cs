using Blazored.Modal.Services;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MealPlanner.UI.Web.Pages
{
    public partial class ShopEdit
    {
        [Parameter]
        public string? Id { get; set; }
        public EditShopModel? Shop { get; set; }

        [Inject]
        public IShopService? ShopService { get; set; }

        [Inject]
        public IProductCategoryService? ProductCategoriesService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public IJSRuntime? JSRuntime { get; set; }

        [CascadingParameter]
        protected IModalService? Modal { get; set; } = default!;

        [CascadingParameter(Name = "ErrorComponent")]
        protected IErrorComponent? ErrorComponent { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _ = int.TryParse(Id, out var id);

            if (id == 0)
            {
                var categories = await ProductCategoriesService!.GetAllAsync();
                Shop = new EditShopModel(categories);
            }
            else
            {
                Shop = await ShopService!.GetEditAsync(id);
            }
        }

        private async void SaveAsync()
        {
            var response = Shop!.Id == 0 ? await ShopService!.AddAsync(Shop) : await ShopService!.UpdateAsync(Shop);
            if (!string.IsNullOrWhiteSpace(response))
            {
                ErrorComponent!.ShowError("Error", response);
            }
            else
            {
                NavigateToOverview();
            }
        }

        private async void DeleteAsync()
        {
            if (Shop!.Id != 0)
            {
                if (!await JSRuntime!.Confirm($"Are you sure you want to delete the shop: '{Shop.Name}'?"))
                    return;

                var response = await ShopService!.DeleteAsync(Shop.Id);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    ErrorComponent!.ShowError("Error", response);
                }
                else
                {
                    NavigateToOverview();
                }
            }
        }

        private void NavigateToOverview()
        {
            NavigationManager!.NavigateTo("/shopsoverview");
        }
    }
}
