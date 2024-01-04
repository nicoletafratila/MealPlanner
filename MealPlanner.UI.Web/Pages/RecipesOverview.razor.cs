using Common.Pagination;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class RecipesOverview
    {
        public PagedList<RecipeModel>? Recipes { get; set; }
        public RecipeModel? Recipe { get; set; }
        public IList<RecipeCategoryModel>? Categories { get; set; }
        public string? CategoryId { get; set; }

        [Inject]
        public IRecipeService? RecipeService { get; set; }

        [Inject]
        public IRecipeCategoryService? CategoryService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public IJSRuntime? JSRuntime { get; set; }

        [Parameter]
        public QueryParameters? QueryParameters { get; set; } = new();

        [CascadingParameter(Name = "ErrorComponent")]
        protected IErrorComponent? ErrorComponent { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Categories = await CategoryService!.GetAllAsync();
            await RefreshAsync();
        }

        private void New()
        {
            NavigationManager!.NavigateTo($"recipeedit/");
        }

        private void Update(RecipeModel item)
        {
            NavigationManager!.NavigateTo($"recipeedit/{item.Id}");
        }

        private async Task DeleteAsync(RecipeModel item)
        {
            if (item != null)
            {
                if (!await JSRuntime!.Confirm($"Are you sure you want to delete the recipe: '{item.Name}'?"))
                    return;
                
                var response = await RecipeService!.DeleteAsync(item.Id);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    ErrorComponent!.ShowError("Error", response);
                }
                else
                {
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
            CategoryId = e!.Value!.ToString();
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
