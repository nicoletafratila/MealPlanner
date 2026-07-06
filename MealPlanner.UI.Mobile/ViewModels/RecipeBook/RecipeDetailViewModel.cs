using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.RecipeBook
{
    [QueryProperty(nameof(RecipeId), "id")]
    public partial class RecipeDetailViewModel(RecipeService recipeService) : BaseViewModel
    {
        [ObservableProperty] private Guid _recipeId;
        // Detail view shows ingredient lines, so we deserialize the edit model
        // (which carries Ingredients). The page only ever reads from it.
        [ObservableProperty] private RecipeEditModel? _recipe;

        partial void OnRecipeIdChanged(Guid value) => _ = LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (RecipeId == Guid.Empty) return;
            IsBusy = true;
            try
            {
                Recipe = await recipeService.GetEditAsync(RecipeId);
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
        private Task EditAsync() => Shell.Current.GoToAsync($"RecipeEdit?id={RecipeId}");
    }
}
