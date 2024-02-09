using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages
{
    public partial class RecipePreview
    {
        [Parameter]
        public int Id { get; set; }
        public EditRecipeModel? Recipe { get; set; }

        [Inject]
        public IRecipeService? RecipeService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            //Recipe = await RecipeService!.GetEditAsync(Id);
            base.OnInitialized();
        }
    }
}
