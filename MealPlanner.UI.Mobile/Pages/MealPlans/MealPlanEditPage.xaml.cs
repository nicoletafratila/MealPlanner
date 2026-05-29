using MealPlanner.UI.Mobile.ViewModels.MealPlans;

namespace MealPlanner.UI.Mobile.Pages.MealPlans
{
    public partial class MealPlanEditPage : ContentPage
    {
        public MealPlanEditPage(MealPlanEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
