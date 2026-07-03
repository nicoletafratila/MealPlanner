using MealPlanner.UI.Mobile.ViewModels.MealPlans;

namespace MealPlanner.UI.Mobile.Pages.MealPlans
{
    public partial class ShoppingListsOverviewPage : ContentPage
    {
        private readonly ShoppingListsOverviewViewModel _vm;

        public ShoppingListsOverviewPage(ShoppingListsOverviewViewModel viewModel)
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
