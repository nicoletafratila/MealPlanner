using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    [QueryProperty(nameof(RecipeId), "id")]
    public partial class RecipeDetailViewModel(RecipeService recipeService) : BaseViewModel
    {
        [ObservableProperty] private string _recipeId = string.Empty;
        // Detail view shows ingredient lines, so we deserialize the edit model
        // (which carries Ingredients). The page only ever reads from it.
        [ObservableProperty] private RecipeEditModel? _recipe;

        partial void OnRecipeIdChanged(string value) => _ = LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (!Guid.TryParse(RecipeId, out var id) || id == Guid.Empty) return;
            IsBusy = true;
            try
            {
                Recipe = await recipeService.GetEditAsync(id);
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task OpenSourceAsync(string? url)
        {
            if (!string.IsNullOrWhiteSpace(url))
                await Launcher.OpenAsync(new Uri(url));
        }

        [RelayCommand]
        private Task EditAsync() => Shell.Current.GoToAsync($"RecipeEdit?id={RecipeId}");
    }
}
