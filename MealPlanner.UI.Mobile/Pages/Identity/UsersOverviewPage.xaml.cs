using MealPlanner.UI.Mobile.ViewModels.Identity;

namespace MealPlanner.UI.Mobile.Pages.Identity
{
    public partial class UsersOverviewPage : ContentPage
    {
        private readonly UsersOverviewViewModel _vm;

        public UsersOverviewPage(UsersOverviewViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _vm = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _vm.LoadCommand.Execute(null);
        }
    }
}
