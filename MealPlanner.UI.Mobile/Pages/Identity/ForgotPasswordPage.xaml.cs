using MealPlanner.UI.Mobile.ViewModels.Identity;

namespace MealPlanner.UI.Mobile.Pages.Identity
{
    public partial class ForgotPasswordPage : ContentPage
    {
        public ForgotPasswordPage(ForgotPasswordViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
