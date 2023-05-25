using Common.Api;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class RecipesOverview
    {
        public IList<RecipeModel>? Recipes { get; set; }
        public RecipeModel? Recipe { get; set; }
        public IList<RecipeCategoryModel>? Categories { get; set; }

        [Inject]
        public IRecipeService? RecipeService { get; set; }

        [Inject]
        public IRecipeCategoryService? CategoryService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public IJSRuntime? JSRuntime { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await RefreshAsync();
        }

        protected void New()
        {
            NavigationManager!.NavigateTo($"recipeedit/");
        }

        protected void Update(RecipeModel item)
        {
            NavigationManager!.NavigateTo($"recipeedit/{item.Id}");
        }

        protected async Task DeleteAsync(RecipeModel item)
        {
            if (item != null)
            {
                if (!await JSRuntime!.Confirm($"Are you sure you want to delete the ingredient: '{item.Name}'?"))
                    return;

                await RecipeService!.DeleteAsync(item.Id);
                await RefreshAsync();
            }
        }

        protected async Task RefreshAsync()
        {
            Recipes = await RecipeService!.GetAllAsync();
            Categories = await CategoryService!.GetAllAsync();
        }

        private async void OnCategoryChangedAsync(ChangeEventArgs e)
        {
            var categoryId = e!.Value!.ToString();
            if (!string.IsNullOrWhiteSpace(categoryId) && categoryId != "0")
                Recipes = await RecipeService!.SearchAsync(int.Parse(categoryId));
            else
                Recipes = await RecipeService!.GetAllAsync();
            StateHasChanged();
        }
    }
}
