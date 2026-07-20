using CommunityToolkit.Maui.Extensions;
using MealPlanner.UI.Mobile.ViewModels.MealPlans;
using MealPlanner.UI.Mobile.Views.Controls;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.Pages.MealPlans
{
    public partial class MealPlanEditPage : ContentPage
    {
        public MealPlanEditPage(MealPlanEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private async void OnSelectRecipeTapped(object sender, TappedEventArgs e)
        {
            if (BindingContext is not MealPlanEditViewModel viewModel)
                return;

            var result = await this.ShowPopupAsync<RecipeModel>(new RecipeSelectorPopup(viewModel.FilteredRecipes));
            if (!result.WasDismissedByTappingOutsideOfPopup && result.Result is { } recipe)
                viewModel.SelectedRecipe = recipe;
        }
    }
}
