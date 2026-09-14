using CommunityToolkit.Maui.Extensions;
using MealPlanner.UI.Mobile.ViewModels.RecipeBook;
using MealPlanner.UI.Mobile.Views.Controls;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.Pages.RecipeBook
{
    public partial class RecipeEditPage : ContentPage
    {
        private readonly RecipeEditViewModel _vm;

        public RecipeEditPage(RecipeEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _vm = viewModel;
        }

        private async void OnSelectProductTapped(object sender, TappedEventArgs e)
        {
            var items = _vm.ProductsByCategory.Select(p => new SelectorItem(p, p.Name, p.EffectiveCategoryName, p.ThumbnailUrl)).ToList();
            var popup = new SelectorPopup(
                items,
                RecipeBook.Resources.RecipeEditPage.SelectProductTitle,
                RecipeBook.Resources.RecipeEditPage.PlaceholderSearchProducts,
                RecipeBook.Resources.RecipeEditPage.EmptyProductsLabel);
            var result = await this.ShowPopupAsync<object>(popup);
            if (!result.WasDismissedByTappingOutsideOfPopup && result.Result is ProductModel product)
                _vm.SelectedProduct = product;
        }

        private async void OnSelectCategoryTapped(object sender, TappedEventArgs e)
        {
            var items = _vm.Categories.Select(c => new SelectorItem(c, c.Name)).ToList();
            var popup = new SelectorPopup(
                items,
                RecipeBook.Resources.RecipeEditPage.SelectCategoryTitle,
                RecipeBook.Resources.RecipeEditPage.PlaceholderSearchCategories,
                RecipeBook.Resources.RecipeEditPage.EmptyCategoriesLabel);
            var result = await this.ShowPopupAsync<object>(popup);
            if (!result.WasDismissedByTappingOutsideOfPopup && result.Result is RecipeCategoryModel category)
                _vm.SelectedCategory = category;
        }

        private async void OnSelectProductCategoryTapped(object sender, TappedEventArgs e)
        {
            var items = _vm.ProductCategories.Select(c => new SelectorItem(c, c.Name)).ToList();
            var popup = new SelectorPopup(
                items,
                RecipeBook.Resources.RecipeEditPage.ProductCategoryTitle,
                RecipeBook.Resources.RecipeEditPage.PlaceholderSearchProductCategories,
                RecipeBook.Resources.RecipeEditPage.EmptyProductCategoriesLabel);
            var result = await this.ShowPopupAsync<object>(popup);
            if (!result.WasDismissedByTappingOutsideOfPopup && result.Result is ProductCategoryModel category)
                _vm.SelectedProductCategory = category;
        }

        private async void OnSelectUnitTapped(object sender, TappedEventArgs e)
        {
            var items = _vm.UnitsForProduct.Select(u => new SelectorItem(u, u.Name)).ToList();
            var popup = new SelectorPopup(
                items,
                RecipeBook.Resources.RecipeEditPage.UnitTitle,
                RecipeBook.Resources.RecipeEditPage.PlaceholderSearchUnits,
                RecipeBook.Resources.RecipeEditPage.EmptyUnitsLabel);
            var result = await this.ShowPopupAsync<object>(popup);
            if (!result.WasDismissedByTappingOutsideOfPopup && result.Result is UnitModel unit)
                _vm.SelectedUnit = unit;
        }
    }
}
