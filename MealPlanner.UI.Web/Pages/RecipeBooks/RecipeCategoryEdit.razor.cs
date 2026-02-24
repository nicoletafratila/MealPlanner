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
        private List<BreadcrumbItem> _navItems = default!;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Parameter]
        public string? Id { get; set; }

        public RecipeCategoryEditModel? RecipeCategory { get; set; }

        [Inject]
        public IRecipeCategoryService RecipeCategoryService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            _navItems = new List<BreadcrumbItem>
            {
                new() { Text = "Recipe categories", Href = "recipebooks/recipecategoriesoverview" },
                new() { Text = "Recipe category", IsCurrentPage = true },
            };

            _ = int.TryParse(Id, out var id);
            if (id == 0)
            {
                RecipeCategory = new RecipeCategoryEditModel();
            }
            else
            {
                RecipeCategory = await RecipeCategoryService.GetEditAsync(id);
            }
        }

        private async Task SaveAsync()
        {
            if (RecipeCategory is null)
                return;

            var isNew = RecipeCategory.Id == 0;

            var response = isNew
                ? await RecipeCategoryService.AddAsync(RecipeCategory)
                : await RecipeCategoryService.UpdateAsync(RecipeCategory);

            if (response != null && !response.Succeeded)
            {
                MessageComponent?.ShowError(response.Message!);
            }
            else
            {
                MessageComponent?.ShowInfo("Data has been saved successfully");
                NavigateToOverview();
            }
        }

        private async Task DeleteAsync()
        {
            if (RecipeCategory is null || RecipeCategory.Id == 0)
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

            var response = await RecipeCategoryService.DeleteAsync(RecipeCategory.Id);
            if (response != null && !response.Succeeded)
            {
                MessageComponent?.ShowError(response.Message!);
            }
            else
            {
                MessageComponent?.ShowInfo("Data has been deleted successfully");
                NavigateToOverview();
            }
        }

        private void NavigateToOverview()
        {
            NavigationManager.NavigateTo("recipebooks/recipecategoriesoverview");
        }
    }
}