using BlazorBootstrap;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class RecipeCategoryEdit
    {
        private List<BreadcrumbItem>? NavItems { get; set; }

        [Parameter]
        public string? Id { get; set; }
        public RecipeCategoryEditModel? RecipeCategory { get; set; }

        [Inject]
        public IRecipeCategoryService? RecipeCategoryService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [CascadingParameter(Name = "MessageComponent")]
        protected IMessageComponent? MessageComponent { get; set; }

        protected ConfirmDialog dialog = default!;

        protected override async Task OnInitializedAsync()
        {
            NavItems = new List<BreadcrumbItem>
            {
                new BreadcrumbItem{ Text = "Recipe categories", Href ="/recipecategoriesoverview" },
                new BreadcrumbItem{ Text = "Recipe category", IsCurrentPage = true },
            };

            _ = int.TryParse(Id, out var id);
            if (id == 0)
            {
                RecipeCategory = new RecipeCategoryEditModel();
            }
            else
            {
                RecipeCategory = await RecipeCategoryService!.GetEditAsync(id);
            }
        }

        private async void SaveAsync()
        {
            var response = RecipeCategory?.Id == 0 ? await RecipeCategoryService!.AddAsync(RecipeCategory) : await RecipeCategoryService!.UpdateAsync(RecipeCategory!);
            if (!string.IsNullOrWhiteSpace(response))
            {
                MessageComponent?.ShowError(response);
            }
            else
            {
                MessageComponent?.ShowInfo("Data has been saved successfully");
                NavigateToOverview();
            }
        }

        private async void DeleteAsync()
        {
            if (RecipeCategory?.Id != 0)
            {
                var options = new ConfirmDialogOptions
                {
                    YesButtonText = "OK",
                    YesButtonColor = ButtonColor.Success,
                    NoButtonText = "Cancel",
                    NoButtonColor = ButtonColor.Danger
                };
                var confirmation = await dialog.ShowAsync(
                        title: "Are you sure you want to delete this?",
                        message1: "This will delete the record. Once deleted can not be rolled back.",
                        message2: "Do you want to proceed?",
                        confirmDialogOptions: options);

                if (!confirmation)
                    return;

                var response = await RecipeCategoryService!.DeleteAsync(RecipeCategory!.Id);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    MessageComponent?.ShowError(response);
                }
                else
                {
                    MessageComponent?.ShowInfo("Data has been deleted successfully");
                    NavigateToOverview();
                }
            }
        }

        private void NavigateToOverview()
        {
            NavigationManager?.NavigateTo("/recipecategoriesoverview");
        }
    }
}
