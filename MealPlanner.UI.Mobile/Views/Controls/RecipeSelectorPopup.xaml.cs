using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.Views.Controls
{
    public partial class RecipeSelectorPopup : Popup<RecipeModel>
    {
        private readonly IReadOnlyList<RecipeModel> _allRecipes;
        private readonly ObservableCollection<RecipeModel> _filteredRecipes;

        public RecipeSelectorPopup(IReadOnlyList<RecipeModel> recipes)
        {
            InitializeComponent();
            _allRecipes = recipes;
            _filteredRecipes = new ObservableCollection<RecipeModel>(recipes);
            RecipesView.ItemsSource = _filteredRecipes;
        }

        private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            var query = e.NewTextValue?.Trim();
            var matches = string.IsNullOrWhiteSpace(query)
                ? _allRecipes
                : _allRecipes.Where(r => r.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

            _filteredRecipes.Clear();
            foreach (var recipe in matches)
                _filteredRecipes.Add(recipe);
        }

        private async void OnRecipeSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is RecipeModel recipe)
                await CloseAsync(recipe, CancellationToken.None);
        }
    }
}
