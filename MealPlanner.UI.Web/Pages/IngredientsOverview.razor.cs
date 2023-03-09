using Common.Api;
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

        [Inject]
        public IIngredientService IngredientService { get; set; }

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
        }
    }
}
