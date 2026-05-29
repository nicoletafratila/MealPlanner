using MealPlanner.UI.Mobile.ViewModels.RecipeBook;

namespace MealPlanner.UI.Mobile.Pages.RecipeBook
{
    public partial class RecipeCategoryEditPage : ContentPage
    {
        public RecipeCategoryEditPage(RecipeCategoryEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
