using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Shared
{
    public partial class RecipePicker
    {
        private string _filterText = string.Empty;

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

        private string FilterText => _filterText;

        private RecipeModel? SelectedRecipe =>
            Recipes?.FirstOrDefault(r => r.Id.ToString() == Value);

        private IEnumerable<RecipeModel>? FilteredRecipes =>
            Recipes is null || string.IsNullOrWhiteSpace(_filterText)
                ? Recipes
                : Recipes.Where(r => r.Name.Contains(_filterText, StringComparison.OrdinalIgnoreCase));

        private void OnFilterInput(ChangeEventArgs e)
        {
            _filterText = e.Value?.ToString() ?? string.Empty;
        }

        private async Task SelectAsync(RecipeModel? item)
        {
            _filterText = string.Empty;
            var newValue = item?.Id.ToString() ?? "0";
            await ValueChanged.InvokeAsync(newValue);
        }
    }
}
