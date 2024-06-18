using BlazorBootstrap;
using Common.Pagination;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class RecipesOverview
    {
        private List<BreadcrumbItem>? NavItems { get; set; }

        [Parameter]
        public QueryParameters? QueryParameters { get; set; } = new();

        public string? CategoryId { get; set; }
        public IList<RecipeCategoryModel>? Categories { get; set; }

        public RecipeModel? Recipe { get; set; }
        public PagedList<RecipeModel>? Recipes { get; set; }

        [Inject]
        public IRecipeService? RecipeService { get; set; }

        [Inject]
        public IRecipeCategoryService? CategoryService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [CascadingParameter(Name = "MessageComponent")]
        protected IMessageComponent? MessageComponent { get; set; }

        protected ConfirmDialog dialog = default!;

        protected override async Task OnInitializedAsync()
        {
            NavItems = new List<BreadcrumbItem>
            {
                new BreadcrumbItem{ Text = "Home", Href ="/" },
                new BreadcrumbItem{ Text = "Recipes", IsCurrentPage = true }
            };
            Categories = await CategoryService!.GetAllAsync();
            await RefreshAsync();
        }

        private void New()
        {
            NavigationManager?.NavigateTo($"recipeedit/");
        }

        private void Update(RecipeModel item)
        {
            NavigationManager?.NavigateTo($"recipeedit/{item.Id}");
        }

        private async void DeleteAsync(RecipeModel item)
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
                var confirmation = await dialog.ShowAsync(
                        title: "Are you sure you want to delete this?",
                        message1: "This will delete the record. Once deleted can not be rolled back.",
                        message2: "Do you want to proceed?",
                        confirmDialogOptions: options);

                if (!confirmation)
                    return;

                var response = await RecipeService!.DeleteAsync(item.Id);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    MessageComponent?.ShowError(response);
                }
                else
                {
                    MessageComponent?.ShowInfo("Data has been deleted successfully");
                    await RefreshAsync();
                }
            }
        }

        private async Task RefreshAsync()
        {
            Recipes = await RecipeService!.SearchAsync(CategoryId, QueryParameters!);
            StateHasChanged();
        }

        private async void OnCategoryChangedAsync(ChangeEventArgs e)
        {
            CategoryId = e?.Value?.ToString();
            QueryParameters = new();
            await RefreshAsync();
        }

        private async Task OnPageChangedAsync(int pageNumber)
        {
            QueryParameters!.PageNumber = pageNumber;
            await RefreshAsync();
        }
    }
}
