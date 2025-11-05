using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Pages.RecipeBooks
{
    [Authorize]
    public partial class RecipePreview
    {
        [Parameter]
        public RecipeEditModel? Recipe { get; set; }
        
        [Parameter]
        public string? RecipeCategory { get; set; }
    }
}
