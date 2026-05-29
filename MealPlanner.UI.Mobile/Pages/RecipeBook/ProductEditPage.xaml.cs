using MealPlanner.UI.Mobile.ViewModels.RecipeBook;

namespace MealPlanner.UI.Mobile.Pages.RecipeBook
{
    public partial class ProductEditPage : ContentPage
    {
        public ProductEditPage(ProductEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
