using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels
{
    public partial class AppShellViewModel(IMealPlanService mealPlanService) : BaseViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasCurrentMealPlan))]
        private MealPlanModel? _currentMealPlan;

        public bool HasCurrentMealPlan => CurrentMealPlan is not null;

        [RelayCommand]
        private async Task LoadCurrentAsync()
        {
            try
            {
                CurrentMealPlan = await mealPlanService.GetCurrentAsync();
            }
            catch
            {
                CurrentMealPlan = null;
            }
        }

        [RelayCommand]
        private Task OpenCurrentAsync()
        {
            var id = CurrentMealPlan?.Id ?? Guid.Empty;
            Shell.Current.FlyoutIsPresented = false;
            return Shell.Current.GoToAsync($"MealPlanEdit?id={id}");
        }
    }
}
