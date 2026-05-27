using BlazorBootstrap;
using Common.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Services;
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
                new() { Text = Resources.RecipeCategoryEdit.BreadcrumbRecipeCategories, Href = "recipebooks/recipecategoriesoverview" },
                new() { Text = Resources.RecipeCategoryEdit.BreadcrumbRecipeCategory, IsCurrentPage = true },
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
                await ShowErrorAsync(Resources.RecipeCategoryEdit.SaveFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.RecipeCategoryEdit.SaveFailed);
                return;
            }

            await ShowInfoAsync(Resources.RecipeCategoryEdit.SaveSucceeded);
            NavigateToOverview();
        }

        private async Task DeleteAsync()
        {
            if (RecipeCategory.Id == 0)
                return;

            var options = new ConfirmDialogOptions
            {
                YesButtonText = Resources.RecipeCategoryEdit.DialogYesButton,
                YesButtonColor = ButtonColor.Success,
                NoButtonText = Resources.RecipeCategoryEdit.DialogNoButton,
                NoButtonColor = ButtonColor.Danger
            };

            var confirmation = await _dialog.ShowAsync(
                title: Resources.RecipeCategoryEdit.DeleteDialogTitle,
                message1: Resources.RecipeCategoryEdit.DeleteDialogMessage1,
                message2: Resources.RecipeCategoryEdit.DeleteDialogMessage2,
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
                await ShowErrorAsync(Resources.RecipeCategoryEdit.DeleteFailedMessage);
                return;
            }

            if (!response.Succeeded)
            {
                await ShowErrorAsync(response.Message ?? Resources.RecipeCategoryEdit.DeleteFailed);
                return;
            }

            await ShowInfoAsync(Resources.RecipeCategoryEdit.DeleteSucceeded);
            NavigateToOverview();
        }

        private void NavigateToOverview()
        {
            NavigationManager.NavigateTo("recipebooks/recipecategoriesoverview");
        }

        private async Task ShowErrorAsync(string message)
            => await MessageComponent!.ShowErrorAsync(message);

        private async Task ShowInfoAsync(string message)
            => await MessageComponent!.ShowInfoAsync(message);
    }
}