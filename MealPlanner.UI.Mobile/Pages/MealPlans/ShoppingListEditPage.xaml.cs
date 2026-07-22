using CommunityToolkit.Maui.Extensions;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Mobile.ViewModels.MealPlans;
using MealPlanner.UI.Mobile.Views.Controls;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.Pages.MealPlans
{
    public partial class ShoppingListEditPage : ContentPage
    {
        private readonly ShoppingListEditViewModel _vm;

        public ShoppingListEditPage(ShoppingListEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _vm = viewModel;
        }

        private async void OnSelectProductTapped(object sender, TappedEventArgs e)
        {
            var items = _vm.ProductsByCategory.Select(p => new SelectorItem(p, p.Name, p.EffectiveCategoryName, p.ImageUrl)).ToList();
            var popup = new SelectorPopup(
                items,
                MealPlans.Resources.ShoppingListEditPage.SelectProductTitle,
                MealPlans.Resources.ShoppingListEditPage.PlaceholderSearchProducts,
                MealPlans.Resources.ShoppingListEditPage.EmptyProductsSearchLabel);
            var result = await this.ShowPopupAsync<object>(popup);
            if (!result.WasDismissedByTappingOutsideOfPopup && result.Result is ProductModel product)
                _vm.SelectedProduct = product;
        }

        private async void OnAddFromMealPlanClicked(object sender, EventArgs e)
        {
            var plans = await _vm.LoadMealPlansForSelectionAsync();
            if (plans is null || plans.Count == 0) return;

            var items = plans.Select(p => new SelectorItem(p, p.Name)).ToList();
            var popup = new SelectorPopup(
                items,
                MealPlans.Resources.ShoppingListEditPage.SelectMealPlanTitle,
                MealPlans.Resources.ShoppingListEditPage.PlaceholderSearchMealPlans,
                MealPlans.Resources.ShoppingListEditPage.EmptyMealPlansLabel);
            var result = await this.ShowPopupAsync<object>(popup);
            if (!result.WasDismissedByTappingOutsideOfPopup && result.Result is MealPlanModel plan)
                await _vm.AddFromMealPlanAsync(plan);
        }

        private async void OnAddFromRecipeClicked(object sender, EventArgs e)
        {
            var recipes = await _vm.LoadRecipesForSelectionAsync();
            if (recipes is null || recipes.Count == 0) return;

            var items = recipes.Select(r => new SelectorItem(r, r.Name, r.EffectiveCategoryName, r.ImageUrl)).ToList();
            var popup = new SelectorPopup(
                items,
                MealPlans.Resources.ShoppingListEditPage.SelectRecipeTitle,
                MealPlans.Resources.ShoppingListEditPage.PlaceholderSearchRecipes,
                MealPlans.Resources.ShoppingListEditPage.EmptyRecipesLabel);
            var result = await this.ShowPopupAsync<object>(popup);
            if (!result.WasDismissedByTappingOutsideOfPopup && result.Result is RecipeModel recipe)
                await _vm.AddFromRecipeAsync(recipe);
        }
    }
}
