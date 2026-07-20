using CommunityToolkit.Maui.Extensions;
using MealPlanner.UI.Mobile.ViewModels.RecipeBook;
using MealPlanner.UI.Mobile.Views.Controls;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.Pages.RecipeBook
{
    public partial class RecipeEditPage : ContentPage
    {
        public RecipeEditPage(RecipeEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private async void OnSelectProductTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is not RecipeIngredientEditViewModel row)
                return;

            var items = row.Products.Select(p => new SelectorItem(p, p.Name, p.EffectiveCategoryName, p.ImageUrl)).ToList();
            var popup = new SelectorPopup(
                items,
                RecipeBook.Resources.RecipeEditPage.SelectProductTitle,
                RecipeBook.Resources.RecipeEditPage.PlaceholderSearchProducts,
                RecipeBook.Resources.RecipeEditPage.EmptyProductsLabel);
            var result = await this.ShowPopupAsync<object>(popup);
            if (!result.WasDismissedByTappingOutsideOfPopup && result.Result is ProductModel product)
                row.SelectedProduct = product;
        }
    }
}
