using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Identity.Services.Core.Http;
using Identity.Shared.Models;
using MealPlanner.UI.Mobile.Services;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class UserProfileViewModel(UserService userService, MobileAuthStateService authState, IdentityService authService) : BaseViewModel
    {
        [ObservableProperty]
        private ApplicationUserEditModel? _model;

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ClearMessages();
            try
            {
                var userName = await authState.GetUserNameAsync();
                if (userName is not null)
                    Model = await userService.GetEditAsync(userName);
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (IsBusy || Model is null) return;
            IsBusy = true;
            ClearMessages();
            try
            {
                var (success, error) = await userService.UpdateAsync(Model);
                if (success) SetSuccess("Profile updated.");
                else SetError(error);
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private Task ChangePasswordAsync() => Shell.Current.GoToAsync("ChangePassword");

        [RelayCommand]
        private async Task LogoutAsync()
        {
            await authService.LogoutAsync();
            Application.Current!.MainPage = new NavigationPage(
                IPlatformApplication.Current!.Services.GetRequiredService<Pages.Identity.LoginPage>());
        }
    }
}
