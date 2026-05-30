using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class LoginViewModel(AuthenticationService authService) : BaseViewModel
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
                var result = await authService.LoginAsync(Model);
                if (result?.Succeeded == true)
                    await Shell.Current.GoToAsync("//RecipesOverview");
                else
                    SetError(result?.Message);
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private Task GoToRegisterAsync() => Shell.Current.GoToAsync("Register");

        [RelayCommand]
        private Task GoToForgotPasswordAsync() => Shell.Current.GoToAsync("ForgotPassword");
    }
}
