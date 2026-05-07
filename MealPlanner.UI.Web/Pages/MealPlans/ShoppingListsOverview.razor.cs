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
using Microsoft.JSInterop;

namespace MealPlanner.UI.Web.Pages.MealPlans
{
    [Authorize]
    public partial class ShoppingListsOverview
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = [];
        private GridTemplate<ShoppingListModel>? _shoppingListsGrid;
        private string _tableGridClass = CssClasses.GridTemplateEmptyClass;
        private bool showCopiedMessage;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Inject]
        public IShoppingListService ShoppingListService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public ISessionStorageService SessionStorage { get; set; } = default!;

        [Inject]
        public IJSRuntime JS { get; set; } = default!;

        protected override void OnInitialized()
        {
            _navItems =
            [
                new BreadcrumbItem { Text = "Home", Href = "recipebooks/recipesoverview" }
            ];
        }

        private void New()
        {
            NavigationManager.NavigateTo("mealplans/shoppinglistedit/");
        }

        private void Update(ShoppingListModel item)
        {
            if (item is null)
                return;

            NavigationManager.NavigateTo($"mealplans/shoppinglistedit/{item.Id}");
        }

        private async Task DeleteAsync(ShoppingListModel item)
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
                message1: "This will delete the record. Once deleted can not be rolled back.",
                message2: "Do you want to proceed?",
                confirmDialogOptions: options);

            if (!confirmation)
                return;

            await DeleteCoreAsync(item);
        }

        private async Task DeleteCoreAsync(ShoppingListModel item)
        {
            var response = await ShoppingListService.DeleteAsync(item.Id);
            if (response is null)
            {
                await ShowErrorAsync("Delete failed. Please try again.");
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? "Delete failed.");
                return;
            }

            await ShowInfoAsync("Data has been deleted successfully");

            if (_shoppingListsGrid is not null)
                await _shoppingListsGrid.RefreshDataAsync();
        }

        private async Task<GridDataProviderResult<ShoppingListModel>> DataProviderAsync(
            GridDataProviderRequest<ShoppingListModel> request)
        {
            var queryParameters = new QueryParameters<ShoppingListModel>
            {
                Filters = request.Filters,
                Sorting = request.Sorting?
                    .Select(QueryParameters<ShoppingListModel>.ToModel)
                    .ToList(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
            };

            var result = await ShoppingListService.SearchAsync(queryParameters)
                         ?? new PagedList<ShoppingListModel>([], new Metadata());

            var items = result.Items ?? [];

            await SessionStorage.SetItemAsync(queryParameters);

            _tableGridClass = items.Count == 0
                ? CssClasses.GridTemplateEmptyClass
                : CssClasses.GridTemplateWithItemsClass + " grid-additional-columns";

            StateHasChanged();

            return new GridDataProviderResult<ShoppingListModel>
            {
                Data = items,
                TotalCount = result.Metadata?.TotalCount ?? 0
            };
        }

        private async Task Export(ShoppingListModel model)
        {
            var text = string.Empty;

            if (model != null)
            {
                var data = await ShoppingListService.GetEditAsync(model.Id);
                if (data is not null && data.Products != null && data.Products.Any())
                {
                    text = string.Join(
                        Environment.NewLine,
                        data.Products.Select(p => p.Product!.Name + " - " + p.Quantity + " " + p.Unit!.Name)
                    );
                }
            }

            await JS.InvokeVoidAsync("copyTextToClipboard", text);

            showCopiedMessage = true;
            StateHasChanged();

            await Task.Delay(2000);

            showCopiedMessage = false;
            StateHasChanged();
        }

        private async Task ShowErrorAsync(string message)
            => await MessageComponent!.ShowErrorAsync(message);

        private async Task ShowInfoAsync(string message)
            => await MessageComponent!.ShowInfoAsync(message);
    }
}