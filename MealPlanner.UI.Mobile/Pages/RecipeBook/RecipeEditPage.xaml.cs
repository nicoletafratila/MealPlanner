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

            var result = await this.ShowPopupAsync<ProductModel>(new ProductSelectorPopup(row.Products));
            if (!result.WasDismissedByTappingOutsideOfPopup && result.Result is { } product)
                row.SelectedProduct = product;
        }
    }
}
