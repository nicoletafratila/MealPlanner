using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;
using Identity.Shared.Resources;
using MealPlanner.Shared.Resources;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class ChangePasswordViewModel(AuthenticationService authService) : BaseViewModel
    {
        public ChangePasswordModel Model { get; } = new();

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task ChangeAsync()
        {
            if (IsBusy) return;
            ClearMessages();

            if (string.IsNullOrWhiteSpace(Model.CurrentPassword))
            {
                SetError(IdentitySharedMessages.CurrentPasswordRequired);
                return;
            }

            if (string.IsNullOrWhiteSpace(Model.NewPassword))
            {
                SetError(IdentitySharedMessages.NewPasswordRequired);
                return;
            }

            if (string.IsNullOrWhiteSpace(Model.ConfirmPassword))
            {
                SetError(IdentitySharedMessages.ConfirmPasswordRequired);
                return;
            }

            if (Model.NewPassword != Model.ConfirmPassword)
            {
                SetError(MealPlannerSharedMessages.NewPasswordsDoNotMatch);
                return;
            }
            IsBusy = true;
            try
            {
                var result = await authService.ChangePasswordAsync(Model);
                if (result?.Succeeded == true)
                {
                    SetSuccess(MealPlannerSharedMessages.PasswordChangedSuccess);
                    await Task.Delay(1500);
                    await Shell.Current.GoToAsync("..");
                }
                else SetError(result?.Message);
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private Task GoBackAsync() => Shell.Current.GoToAsync("..");
    }
}
