using BlazorBootstrap;
using Common.Models;
using Common.UI;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
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
                new BreadcrumbItem { Text = "Home", Href = "recipebooks/recipesoverview" }
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

        private async Task DeleteCoreAsync(RecipeCategoryModel item)
        {
            var response = await RecipeCategoriesService.DeleteAsync(item.Id);
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
            await RefreshAsync();
        }

        private async Task SaveAsync()
        {
            if (Categories is null || Categories.Count == 0)
            {
                ShowError("There are no categories to save.");
                return;
            }

            var response = await RecipeCategoriesService.UpdateAsync(Categories);
            if (response is null)
            {
                ShowError("Save failed. Please try again.");
                return;
            }

            if (!response.Succeeded)
            {
                ShowError(response.Message ?? "Save failed.");
                return;
            }

            ShowInfo("Data has been saved successfully");
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

        private void ShowError(string message)
            => MessageComponent?.ShowError(message);

        private void ShowInfo(string message)
            => MessageComponent?.ShowInfo(message);
    }
}