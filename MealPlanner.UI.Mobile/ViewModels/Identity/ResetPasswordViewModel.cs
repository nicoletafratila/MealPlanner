using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class ResetPasswordViewModel(AuthenticationService authService) : BaseViewModel
    {
        public ResetPasswordModel Model { get; } = new();

        [RelayCommand]
        private async Task ResetAsync()
        {
            if (IsBusy) return;
            ClearMessages();
            if (Model.NewPassword != Model.ConfirmPassword)
            {
                SetError("Passwords do not match.");
                return;
            }
            IsBusy = true;
            try
            {
                var result = await authService.ResetPasswordAsync(Model);
                if (result?.Succeeded == true)
                {
                    SetSuccess("Password reset successfully. Please log in.");
                    await Task.Delay(1500);
                    await Shell.Current.GoToAsync("..");
                }
                else SetError(result?.Message);
            }
            finally { IsBusy = false; }
        }
    }
}
