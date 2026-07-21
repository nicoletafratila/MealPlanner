using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;
using Identity.Shared.Resources;
using MealPlanner.UI.Mobile.Services;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class LoginViewModel(AuthenticationService authService, SecureStorageTokenProvider tokenProvider) : BaseViewModel
    {
        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _rememberMe;

        public async Task InitializeAsync()
        {
            var (username, password) = await tokenProvider.GetCredentialsAsync();
            if (username != null)
            {
                Username = username;
                Password = password ?? string.Empty;
                RememberMe = true;
            }
        }

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
                var result = await authService.LoginAsync(new LoginModel { Username = Username, Password = Password });
                if (result?.Succeeded == true)
                {
                    if (RememberMe)
                        await tokenProvider.SaveCredentialsAsync(Username, Password);
                    else
                        tokenProvider.ClearCredentials();
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
