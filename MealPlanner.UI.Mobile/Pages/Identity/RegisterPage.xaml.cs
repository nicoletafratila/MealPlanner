using MealPlanner.UI.Mobile.ViewModels.Identity;

namespace MealPlanner.UI.Mobile.Pages.Identity
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage(RegisterViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
