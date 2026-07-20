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

            var items = viewModel.FilteredRecipes.Select(r => new SelectorItem(r, r.Name, r.EffectiveCategoryName, r.ImageUrl)).ToList();
            var popup = new SelectorPopup(
                items,
                MealPlanner.UI.Mobile.Pages.MealPlans.Resources.MealPlanEditPage.SelectRecipeTitle,
                MealPlanner.UI.Mobile.Pages.MealPlans.Resources.MealPlanEditPage.PlaceholderSearchRecipes,
                MealPlanner.UI.Mobile.Pages.MealPlans.Resources.MealPlanEditPage.EmptyRecipesSearchLabel);
            var result = await this.ShowPopupAsync<object>(popup);
            if (!result.WasDismissedByTappingOutsideOfPopup && result.Result is RecipeModel recipe)
                viewModel.SelectedRecipe = recipe;
        }
    }
}
