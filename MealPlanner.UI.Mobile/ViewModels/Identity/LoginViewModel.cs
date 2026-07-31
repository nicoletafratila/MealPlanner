using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;
using Identity.Shared.Resources;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class LoginViewModel(IAuthenticationService authService, AppShellViewModel appShellViewModel) : BaseViewModel
    {
        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _rememberMe;

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task LoginAsync()
        {
            if (IsBusy) return;
            ClearMessages();

            if (string.IsNullOrWhiteSpace(Username))
            {
                SetError(IdentitySharedMessages.UsernameRequired);
                return;
            }

            IsBusy = true;
            try
            {
                var result = await authService.LoginAsync(new LoginModel { Username = Username, Password = Password, RememberLogin = RememberMe });
                if (result?.Succeeded == true)
                {
                    await appShellViewModel.LoadCurrentCommand.ExecuteAsync(null);
                    await Shell.Current.GoToAsync("//RecipesOverview");
                }
                else
                    SetError(result?.Message);
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private Task GoToRegisterAsync() => Shell.Current.GoToAsync("Register");

        [RelayCommand]
        private Task GoToForgotPasswordAsync() => Shell.Current.GoToAsync("ForgotPassword");
    }
}
