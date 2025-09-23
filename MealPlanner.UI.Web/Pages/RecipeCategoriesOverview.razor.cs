using BlazorBootstrap;
using Common.Models;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    [Authorize]
    public partial class RecipeCategoriesOverview
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = default!;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        public IList<RecipeCategoryModel>? Categories { get; set; }

        [Inject]
        public IRecipeCategoryService? RecipeCategoriesService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }
  
        protected override async Task OnInitializedAsync()
        {
            _navItems = new List<BreadcrumbItem>
            {
               new BreadcrumbItem{ Text = "Home", Href ="/" }
            };
            await RefreshAsync();
        }

        private void New()
        {
            NavigationManager?.NavigateTo($"recipecategoryedit/");
        }

        private void Update(RecipeCategoryModel item)
        {
            NavigationManager?.NavigateTo($"recipecategoryedit/{item.Id}");
        }

        private async Task DeleteAsync(RecipeCategoryModel item)
        {
            if (item != null)
            {
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

                var response = await RecipeCategoriesService!.DeleteAsync(item.Id);
                if (response != null && !response.Succeeded)
                {
                    MessageComponent?.ShowError(response.Message!);
                }
                else
                {
                    MessageComponent?.ShowInfo("Data has been deleted successfully");
                    await RefreshAsync();
                }
            }
        }

        private async Task SaveAsync()
        {
            var response = await RecipeCategoriesService!.UpdateAsync(Categories!);
            if (response != null && !response.Succeeded)
            {
                MessageComponent?.ShowError(response.Message!);
            }
            else
            {
                MessageComponent?.ShowInfo("Data has been saved successfully");
                await RefreshAsync();
            }
        }

        private bool CanMoveUp(RecipeCategoryModel item)
        {
            return Categories!.IndexOf(item) - 1 >= 0;
        }

        private void MoveUp(RecipeCategoryModel item)
        {
            int index = Categories!.IndexOf(item);
            Categories!.RemoveAt(index);
            if (index - 1 >= 0)
            {
                Categories!.Insert(index - 1, item);
            }
            Categories!.SetIndexes();
        }

        private bool CanMoveDown(RecipeCategoryModel item)
        {
            return Categories!.IndexOf(item) + 2 <= Categories!.Count;
        }

        private void MoveDown(RecipeCategoryModel item)
        {
            int index = Categories!.IndexOf(item);
            Categories!.RemoveAt(index);
            if (index + 1 <= Categories!.Count)
            {
                Categories!.Insert(index + 1, item);
            }
            Categories!.SetIndexes();
        }

        private async Task RefreshAsync()
        {
            var result = await RecipeCategoriesService!.SearchAsync();
            Categories = result != null ? result.Items : new List<RecipeCategoryModel>();
            StateHasChanged();
        }
    }
}
