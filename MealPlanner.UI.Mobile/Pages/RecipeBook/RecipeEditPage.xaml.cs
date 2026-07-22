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
            var items = _vm.ProductsByCategory.Select(p => new SelectorItem(p, p.Name, p.EffectiveCategoryName, p.ImageUrl)).ToList();
            var popup = new SelectorPopup(
                items,
                RecipeBook.Resources.RecipeEditPage.SelectProductTitle,
                RecipeBook.Resources.RecipeEditPage.PlaceholderSearchProducts,
                RecipeBook.Resources.RecipeEditPage.EmptyProductsLabel);
            var result = await this.ShowPopupAsync<object>(popup);
            if (!result.WasDismissedByTappingOutsideOfPopup && result.Result is ProductModel product)
                _vm.SelectedProduct = product;
        }
    }
}
