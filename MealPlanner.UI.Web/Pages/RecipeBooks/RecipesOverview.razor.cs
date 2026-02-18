using BlazorBootstrap;
using Blazored.SessionStorage;
using Common.Constants;
using Common.Pagination;
using Common.UI;
using MealPlanner.UI.Web.Services.Identities;
using MealPlanner.UI.Web.Services.RecipeBooks;
using MealPlanner.UI.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages.RecipeBooks
{
    [Authorize]
    public partial class RecipesOverview
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = [];
        private GridTemplate<RecipeModel>? _recipesGrid;
        private string _tableGridClass = CssClasses.GridTemplateEmptyHorizontalClass;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Inject]
        public IRecipeService RecipeService { get; set; } = default!;

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
            NavigationManager.NavigateTo("recipebooks/recipeedit/");
        }

        private void Update(RecipeModel item)
        {
            if (item is null)
                return;

            NavigationManager.NavigateTo($"recipebooks/recipeedit/{item.Id}");
        }

        private async Task DeleteAsync(RecipeModel item)
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

        private async Task DeleteCoreAsync(RecipeModel item)
        {
            var response = await RecipeService.DeleteAsync(item.Id);
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

            if (_recipesGrid is not null)
                await _recipesGrid.RefreshDataAsync();
        }

        private async Task<GridDataProviderResult<RecipeModel>> DataProviderAsync(GridDataProviderRequest<RecipeModel> request)
        {
            var queryParameters = new QueryParameters<RecipeModel>
            {
                Filters = request.Filters,
                Sorting = request.Sorting?
                    .Select(QueryParameters<RecipeModel>.ToModel)
                    .ToList(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var result = await RecipeService.SearchAsync(queryParameters)
                         ?? new PagedList<RecipeModel>([], new Metadata());

            var items = result.Items ?? [];

            await SessionStorage.SetItemAsync(queryParameters);

            _tableGridClass = items.Count == 0
                ? CssClasses.GridTemplateEmptyHorizontalClass
                : CssClasses.GridTemplateWithItemsHorizontalClass;

            return new GridDataProviderResult<RecipeModel>
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