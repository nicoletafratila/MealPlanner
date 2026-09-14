using CommunityToolkit.Maui.Extensions;
using MealPlanner.UI.Mobile.ViewModels.RecipeBook;
using MealPlanner.UI.Mobile.Views.Controls;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.Pages.RecipeBook
{
    public partial class RecipesOverviewPage : ContentPage
    {
        private readonly RecipesOverviewViewModel _vm;

        public RecipesOverviewPage(RecipesOverviewViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _vm = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _vm.LoadCommand.Execute(null);
        }

        private async void OnSelectCategoryTapped(object sender, TappedEventArgs e)
        {
            var items = _vm.Categories.Where(c => c.Id != Guid.Empty).Select(c => new SelectorItem(c, c.Name)).ToList();
            var popup = new SelectorPopup(
                items,
                RecipeBook.Resources.RecipesOverviewPage.AllCategoriesTitle,
                RecipeBook.Resources.RecipesOverviewPage.PlaceholderSearchCategories,
                RecipeBook.Resources.RecipesOverviewPage.EmptyCategoriesLabel);
            var result = await this.ShowPopupAsync<object>(popup);
            if (!result.WasDismissedByTappingOutsideOfPopup && result.Result is RecipeCategoryModel category)
                _vm.SelectedCategory = category;
        }
    }
}
