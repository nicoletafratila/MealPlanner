using MealPlanner.UI.Mobile.ViewModels.RecipeBook;

namespace MealPlanner.UI.Mobile.Pages.RecipeBook
{
    public partial class RecipesOverviewPage : ContentPage
    {
        private readonly RecipesOverviewViewModel _vm;

        public RecipesOverviewPage(RecipesOverviewViewModel viewModel)
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
