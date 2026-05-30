using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class LoginViewModel(IdentityService authService) : BaseViewModel
    {
        public LoginModel Model { get; } = new();

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsBusy) return;
            ClearMessages();
            IsBusy = true;
            try
            {
                var (success, error) = await authService.LoginAsync(Model);
                if (success)
                    await Shell.Current.GoToAsync("//RecipesOverview");
                else
                    SetError(error);
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private Task GoToRegisterAsync() => Shell.Current.GoToAsync("Register");

        [RelayCommand]
        private Task GoToForgotPasswordAsync() => Shell.Current.GoToAsync("ForgotPassword");
    }
}
