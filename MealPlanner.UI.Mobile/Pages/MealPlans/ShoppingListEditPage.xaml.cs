using CommunityToolkit.Maui.Extensions;
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
    }
}
