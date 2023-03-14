using Common.Api;
using Common.Data.Entities;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class IngredientsOverview
    {
        public IList<IngredientModel> Ingredients { get; set; } = new List<IngredientModel>();
        public IngredientModel Ingredient { get; set; } = new IngredientModel();
        public IList<IngredientCategoryModel> Categories { get; set; } = new List<IngredientCategoryModel>();

        [Inject]
        public IIngredientService IngredientService { get; set; }

        [Inject]
        public IIngredientCategoryService CategoryService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await RefreshAsync();
        }

        protected void New()
        {
            NavigationManager.NavigateTo($"ingredientedit/");
        }

        protected void Update(IngredientModel item)
        {
            NavigationManager.NavigateTo($"ingredientedit/{item.Id}");
        }

        protected async Task DeleteAsync(IngredientModel item)
        {
            if (item != null)
            {
                if (!await JSRuntime.Confirm($"Are you sure you want to delete the ingredient: '{item.Name}'?"))
                    return;

                await IngredientService.DeleteAsync(item.Id);
                await RefreshAsync();
            }
        }

        protected async Task RefreshAsync()
        {
            Ingredients = await IngredientService.GetAllAsync();
            Categories = await CategoryService.GetAllAsync();
        }

        private async void OnCategoryChangedAsync(ChangeEventArgs e)
        {
            var categoryId = e.Value.ToString();
            if (!string.IsNullOrWhiteSpace(categoryId) && categoryId != "0")
                Ingredients = await IngredientService.SearchAsync(int.Parse(categoryId));
            else
                Ingredients = await IngredientService.GetAllAsync();
            StateHasChanged();
        }
    }
}
