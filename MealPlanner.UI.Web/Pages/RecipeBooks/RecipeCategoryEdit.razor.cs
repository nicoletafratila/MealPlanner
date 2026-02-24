using BlazorBootstrap;
using Common.UI;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages.RecipeBooks
{
    [Authorize]
    public partial class RecipeCategoryEdit
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = [];

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Parameter]
        public string? Id { get; set; }

        public RecipeCategoryEditModel RecipeCategory { get; set; } = new();

        [Inject]
        public IRecipeCategoryService RecipeCategoryService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            _navItems =
            [
                new() { Text = "Recipe categories", Href = "recipebooks/recipecategoriesoverview" },
                new() { Text = "Recipe category", IsCurrentPage = true },
            ];

            if (!int.TryParse(Id, out var id) || id == 0)
            {
                RecipeCategory = new RecipeCategoryEditModel();
            }
            else
            {
                RecipeCategory = await RecipeCategoryService.GetEditAsync(id)
                                 ?? new RecipeCategoryEditModel { Id = id };
            }
        }

        private async Task SaveAsync()
        {
            await SaveCoreAsync(RecipeCategory);
        }

        private async Task SaveCoreAsync(RecipeCategoryEditModel recipeCategory)
        {
            Common.Models.CommandResponse? response;

            if (recipeCategory.Id == 0)
            {
                response = await RecipeCategoryService.AddAsync(recipeCategory);
            }
            else
            {
                response = await RecipeCategoryService.UpdateAsync(recipeCategory);
            }

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
            NavigateToOverview();
        }

        private async Task DeleteAsync()
        {
            if (RecipeCategory.Id == 0)
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

            await DeleteCoreAsync(RecipeCategory);
        }

        private async Task DeleteCoreAsync(RecipeCategoryEditModel recipeCategory)
        {
            if (recipeCategory.Id == 0)
                return;

            var response = await RecipeCategoryService.DeleteAsync(recipeCategory.Id);
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
            NavigateToOverview();
        }

        private void NavigateToOverview()
        {
            NavigationManager.NavigateTo("recipebooks/recipecategoriesoverview");
        }

        private void ShowError(string message)
            => MessageComponent?.ShowError(message);

        private void ShowInfo(string message)
            => MessageComponent?.ShowInfo(message);
    }
}