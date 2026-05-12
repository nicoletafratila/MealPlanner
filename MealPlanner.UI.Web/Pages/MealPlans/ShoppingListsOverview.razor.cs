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
                new BreadcrumbItem { Text = Resources.ShoppingListsOverview.BreadcrumbHome, Href = "recipebooks/recipesoverview" }
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
                YesButtonText = Resources.ShoppingListsOverview.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.ShoppingListsOverview.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.ShoppingListsOverview.DeleteDialogTitle,
                message1: Resources.ShoppingListsOverview.DeleteDialogMessage1,
                message2: Resources.ShoppingListsOverview.DeleteDialogMessage2,
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
                await ShowErrorAsync(Resources.ShoppingListsOverview.DeleteFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.ShoppingListsOverview.DeleteFailed);
                return;
            }

            await ShowInfoAsync(Resources.ShoppingListsOverview.DeleteSucceeded);

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