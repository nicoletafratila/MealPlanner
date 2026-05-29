using MealPlanner.UI.Mobile.ViewModels.MealPlans;

namespace MealPlanner.UI.Mobile.Pages.MealPlans
{
    public partial class ShopsOverviewPage : ContentPage
    {
        private readonly ShopsOverviewViewModel _vm;

        public ShopsOverviewPage(ShopsOverviewViewModel viewModel)
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
