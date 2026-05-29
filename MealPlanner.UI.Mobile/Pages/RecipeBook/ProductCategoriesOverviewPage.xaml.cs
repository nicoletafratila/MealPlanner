using MealPlanner.UI.Mobile.ViewModels.RecipeBook;

namespace MealPlanner.UI.Mobile.Pages.RecipeBook
{
    public partial class ProductCategoriesOverviewPage : ContentPage
    {
        private readonly ProductCategoriesViewModel _vm;

        public ProductCategoriesOverviewPage(ProductCategoriesViewModel viewModel)
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
