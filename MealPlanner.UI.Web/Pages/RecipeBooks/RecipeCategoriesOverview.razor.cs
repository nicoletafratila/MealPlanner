using BlazorBootstrap;
using Common.Models;
using Common.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages.RecipeBooks
{
    [Authorize]
    public partial class RecipeCategoriesOverview
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = [];

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        public IList<RecipeCategoryModel> Categories { get; private set; } = [];

        [Inject]
        public IRecipeCategoryService RecipeCategoriesService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            _navItems =
            [
                new BreadcrumbItem { Text = Resources.RecipeCategoriesOverview.BreadcrumbHome, Href = "recipebooks/recipesoverview" }
            ];

            await RefreshAsync();
        }

        private void New()
        {
            NavigationManager.NavigateTo("recipebooks/recipecategoryedit/");
        }

        private void Update(RecipeCategoryModel item)
        {
            if (item is null)
                return;

            NavigationManager.NavigateTo($"recipebooks/recipecategoryedit/{item.Id}");
        }

        private async Task DeleteAsync(RecipeCategoryModel item)
        {
            if (item is null)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = Resources.RecipeCategoriesOverview.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.RecipeCategoriesOverview.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.RecipeCategoriesOverview.DeleteDialogTitle,
                message1: Resources.RecipeCategoriesOverview.DeleteDialogMessage1,
                message2: Resources.RecipeCategoriesOverview.DeleteDialogMessage2,
                confirmDialogOptions: options);

            if (!confirmation)
                return;

            await DeleteCoreAsync(item);
        }

        private async Task DeleteCoreAsync(RecipeCategoryModel item)
        {
            var response = await RecipeCategoriesService.DeleteAsync(item.Id);
            if (response is null)
            {
                await ShowErrorAsync(Resources.RecipeCategoriesOverview.DeleteFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.RecipeCategoriesOverview.DeleteFailed);
                return;
            }

            await ShowInfoAsync(Resources.RecipeCategoriesOverview.DeleteSucceeded);
            await RefreshAsync();
        }

        private async Task SaveAsync()
        {
            if (Categories is null || Categories.Count == 0)
            {
                await ShowErrorAsync(Resources.RecipeCategoriesOverview.NoCategoriesMessage);
                return;
            }

            var response = await RecipeCategoriesService.UpdateAsync(Categories);
            if (response is null)
            {
                await ShowErrorAsync(Resources.RecipeCategoriesOverview.SaveFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.RecipeCategoriesOverview.SaveFailed);
                return;
            }

            await ShowInfoAsync(Resources.RecipeCategoriesOverview.SaveSucceeded);
            await RefreshAsync();
        }

        private bool CanMoveUp(RecipeCategoryModel item)
        {
            if (Categories is null)
                return false;

            var index = Categories.IndexOf(item);
            return index > 0;
        }

        private void MoveUp(RecipeCategoryModel item)
        {
            if (Categories is null)
                return;

            var index = Categories.IndexOf(item);
            if (index <= 0)
                return;

            Categories.RemoveAt(index);
            Categories.Insert(index - 1, item);
            Categories.SetIndexes();
        }

        private bool CanMoveDown(RecipeCategoryModel item)
        {
            if (Categories is null)
                return false;

            var index = Categories.IndexOf(item);
            return index >= 0 && index < Categories.Count - 1;
        }

        private void MoveDown(RecipeCategoryModel item)
        {
            if (Categories is null)
                return;

            var index = Categories.IndexOf(item);
            if (index < 0 || index >= Categories.Count - 1)
                return;

            Categories.RemoveAt(index);
            Categories.Insert(index + 1, item);
            Categories.SetIndexes();
        }

        private async Task RefreshAsync()
        {
            var result = await RecipeCategoriesService.SearchAsync();
            Categories = result?.Items ?? [];
            StateHasChanged();
        }

        private async Task ShowErrorAsync(string message)
            => await MessageComponent!.ShowErrorAsync(message);

        private async Task ShowInfoAsync(string message)
            => await MessageComponent!.ShowInfoAsync(message);
    }
}