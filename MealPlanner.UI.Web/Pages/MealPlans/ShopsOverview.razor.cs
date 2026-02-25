using BlazorBootstrap;
using Blazored.SessionStorage;
using Common.Constants;
using Common.Pagination;
using Common.UI;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services.Identities;
using MealPlanner.UI.Web.Services.MealPlans;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages.MealPlans
{
    [Authorize]
    public partial class ShopsOverview
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = new();
        private GridTemplate<ShopModel>? _shopsGrid;
        private string _tableGridClass = CssClasses.GridTemplateEmptyClass;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Inject]
        public IShopService ShopService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public ISessionStorageService SessionStorage { get; set; } = default!;

        protected override void OnInitialized()
        {
            _navItems =
            [
                new BreadcrumbItem { Text = "Home", Href = "recipebooks/recipesoverview" }
            ];
        }

        private void New()
        {
            NavigationManager.NavigateTo("mealplans/shopedit/");
        }

        private void Update(ShopModel item)
        {
            if (item is null)
                return;

            NavigationManager.NavigateTo($"mealplans/shopedit/{item.Id}");
        }

        private async Task DeleteAsync(ShopModel item)
        {
            if (item is null)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = "OK",
                YesButtonColor = ButtonColor.Success,
                NoButtonText = "Cancel",
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: "Are you sure you want to delete this?",
                message1: "This will delete the record. Once deleted it cannot be rolled back.",
                message2: "Do you want to proceed?",
                confirmDialogOptions: options);

            if (!confirmation)
                return;

            await DeleteCoreAsync(item);
        }

        private async Task DeleteCoreAsync(ShopModel item)
        {
            var response = await ShopService.DeleteAsync(item.Id);
            if (response is null)
            {
                ShowError("Delete failed. Please try again.");
                return;
            }

            if (!response.Succeeded)
            {
                ShowError(response.Message ?? "Delete failed.");
                return;
            }

            ShowInfo("Data has been deleted successfully");

            if (_shopsGrid is not null)
                await _shopsGrid.RefreshDataAsync();
        }

        private async Task<GridDataProviderResult<ShopModel>> DataProviderAsync(
            GridDataProviderRequest<ShopModel> request)
        {
            var queryParameters = new QueryParameters<ShopModel>
            {
                Filters = request.Filters,
                Sorting = request.Sorting?
                    .Select(QueryParameters<ShopModel>.ToModel)
                    .ToList(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };

            var result = await ShopService.SearchAsync(queryParameters)
                         ?? new PagedList<ShopModel>([], new Metadata());

            var items = result.Items ?? [];

            await SessionStorage.SetItemAsync(queryParameters);

            _tableGridClass = items.Count == 0
                ? CssClasses.GridTemplateEmptyClass
                : CssClasses.GridTemplateWithItemsClass + " grid-additional-columns";

            StateHasChanged();

            return new GridDataProviderResult<ShopModel>
            {
                Data = items,
                TotalCount = result.Metadata?.TotalCount ?? 0
            };
        }

        private void ShowError(string message)
            => MessageComponent?.ShowError(message);

        private void ShowInfo(string message)
            => MessageComponent?.ShowInfo(message);
    }
}