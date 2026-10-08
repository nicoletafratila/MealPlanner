using CommunityToolkit.Maui.Extensions;
using Identity.Shared.Models;
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
            var items = _vm.ProductsByCategory.Select(p => new SelectorItem(p, p.Name, p.ProductCategoryName, p.ThumbnailUrl)).ToList();
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

        private async void OnShareTapped(object sender, EventArgs e)
        {
            var users = await _vm.GetShareUsersAsync();
            if (users.Count == 0)
            {
                await this.DisplayAlertAsync(
                    RecipeBook.Resources.RecipeEditPage.SelectShareUserTitle,
                    RecipeBook.Resources.RecipeEditPage.ShareNoUsersMessage,
                    RecipeBook.Resources.RecipeEditPage.OkButton);
                return;
            }

            var items = users.Select(u => new SelectorItem(u, DisplayName(u), u.Username)).ToList();
            var popup = new SelectorPopup(
                items,
                RecipeBook.Resources.RecipeEditPage.SelectShareUserTitle,
                RecipeBook.Resources.RecipeEditPage.PlaceholderSearchUsers,
                RecipeBook.Resources.RecipeEditPage.EmptyUsersLabel);
            var result = await this.ShowPopupAsync<object>(popup);
            if (!result.WasDismissedByTappingOutsideOfPopup && result.Result is ApplicationUserListModel user)
                await _vm.ShareToUserAsync(user.UserId);
        }

        private static string DisplayName(ApplicationUserListModel user)
        {
            var fullName = string.Join(" ", new[] { user.FirstName, user.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
            return string.IsNullOrWhiteSpace(fullName) ? user.Username : $"{fullName} ({user.Username})";
        }
    }
}
