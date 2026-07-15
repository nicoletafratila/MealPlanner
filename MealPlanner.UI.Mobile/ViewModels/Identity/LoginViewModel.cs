using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;
using MealPlanner.UI.Mobile.Services;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class LoginViewModel(AuthenticationService authService, SecureStorageTokenProvider tokenProvider) : BaseViewModel
    {
        public LoginModel Model { get; } = new();

        [ObservableProperty]
        private bool _rememberMe;

        public async Task InitializeAsync()
        {
            var (username, _) = await tokenProvider.GetCredentialsAsync();
            RememberMe = username != null;
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task LoginAsync()
        {
            if (IsBusy) return;
            ClearMessages();
            IsBusy = true;
            try
            {
                var result = await authService.LoginAsync(Model);
                if (result?.Succeeded == true)
                {
                    if (RememberMe)
                        await tokenProvider.SaveCredentialsAsync(Model.Username, Model.Password ?? string.Empty);
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
