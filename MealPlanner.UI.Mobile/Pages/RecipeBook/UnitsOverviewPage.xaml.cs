using MealPlanner.UI.Mobile.ViewModels.RecipeBook;

namespace MealPlanner.UI.Mobile.Pages.RecipeBook
{
    public partial class UnitsOverviewPage : ContentPage
    {
        private readonly UnitsOverviewViewModel _vm;

        public UnitsOverviewPage(UnitsOverviewViewModel viewModel)
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
