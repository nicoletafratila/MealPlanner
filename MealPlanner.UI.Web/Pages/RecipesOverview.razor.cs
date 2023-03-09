using Common.Api;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class RecipesOverview
    {
        public IList<RecipeModel> Recipes { get; set; } = new List<RecipeModel>();
        public RecipeModel Recipe { get; set; } = new RecipeModel();

        [Inject]
        public IRecipeService RecipeService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await Refresh();
        }

        protected void New()
        {
            NavigationManager.NavigateTo($"recipeedit/");
        }

        protected void Update(RecipeModel item)
        {
            NavigationManager.NavigateTo($"recipeedit/{item.Id}");
        }

        protected async Task Delete(RecipeModel item)
        {
            if (item != null)
            {
                if (!await JSRuntime.Confirm($"Are you sure you want to delete the ingredient: '{item.Name}'?"))
                    return;

                await RecipeService.DeleteAsync(item.Id);
                await Refresh();
            }
        }

        protected async Task Refresh()
        {
            Recipes = await RecipeService.GetAll();
        }
    }
}
