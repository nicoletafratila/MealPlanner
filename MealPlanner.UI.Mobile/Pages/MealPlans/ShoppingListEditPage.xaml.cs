using MealPlanner.Shared.Models;
using MealPlanner.UI.Mobile.ViewModels.MealPlans;

namespace MealPlanner.UI.Mobile.Pages.MealPlans
{
    public partial class ShoppingListEditPage : ContentPage
    {
        private readonly ShoppingListEditViewModel _vm;

        public ShoppingListEditPage(ShoppingListEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _vm = viewModel;
        }

        private void OnProductCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox cb && cb.BindingContext is ShoppingListProductEditModel product)
                _vm.ToggleProductCollectedCommand.Execute(product);
        }
    }
}
