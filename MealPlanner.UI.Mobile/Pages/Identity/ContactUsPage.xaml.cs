using MealPlanner.UI.Mobile.ViewModels.Identity;

namespace MealPlanner.UI.Mobile.Pages.Identity
{
    public partial class ContactUsPage : ContentPage
    {
        public ContactUsPage(ContactUsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
