using MealPlanner.UI.Mobile.ViewModels.Identity;

namespace MealPlanner.UI.Mobile.Pages.Identity
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
