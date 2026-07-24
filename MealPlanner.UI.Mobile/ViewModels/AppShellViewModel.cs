using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Mobile.Services;

namespace MealPlanner.UI.Mobile.ViewModels
{
    public partial class AppShellViewModel(IMealPlanService mealPlanService, MobileAuthStateService authStateService) : BaseViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasCurrentMealPlan))]
        private MealPlanModel? _currentMealPlan;

        [ObservableProperty]
        private bool _isAdmin;

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

            try
            {
                var user = await authStateService.GetCurrentUserAsync();
                IsAdmin = user.IsInRole("admin");
            }
            catch
            {
                IsAdmin = false;
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
