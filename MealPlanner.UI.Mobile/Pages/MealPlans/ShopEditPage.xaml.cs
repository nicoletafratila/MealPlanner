using MealPlanner.UI.Mobile.ViewModels.MealPlans;

namespace MealPlanner.UI.Mobile.Pages.MealPlans
{
    public partial class ShopEditPage : ContentPage
    {
        public ShopEditPage(ShopEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
