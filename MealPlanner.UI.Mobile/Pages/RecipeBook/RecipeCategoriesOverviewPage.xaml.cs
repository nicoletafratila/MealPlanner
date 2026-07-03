using MealPlanner.UI.Mobile.ViewModels.RecipeBook;

namespace MealPlanner.UI.Mobile.Pages.RecipeBook
{
    public partial class RecipeCategoriesOverviewPage : ContentPage
    {
        private readonly RecipeCategoriesViewModel _vm;

        public RecipeCategoriesOverviewPage(RecipeCategoriesViewModel viewModel)
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
