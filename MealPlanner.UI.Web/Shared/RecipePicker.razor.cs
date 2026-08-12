using Microsoft.AspNetCore.Components;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Shared
{
    public partial class RecipePicker
    {
        [Parameter]
        public string Id { get; set; } = default!;

        [Parameter]
        public IEnumerable<RecipeModel>? Recipes { get; set; }

        [Parameter]
        public string? Value { get; set; }

        [Parameter]
        public EventCallback<string?> ValueChanged { get; set; }

        [Parameter]
        public string Placeholder { get; set; } = default!;

        private RecipeModel? SelectedRecipe =>
            Recipes?.FirstOrDefault(r => r.Id.ToString() == Value);

        private async Task SelectAsync(RecipeModel? item)
        {
            var newValue = item?.Id.ToString() ?? "0";
            await ValueChanged.InvokeAsync(newValue);
        }
    }
}
