using Common.Constants;
using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class ForgotPasswordViewModel(AuthenticationService authService) : BaseViewModel
    {
        public ForgotPasswordModel Model { get; } = new() { Source = InputSource.Mobile };

        [RelayCommand]
        private async Task SendAsync()
        {
            if (IsBusy) return;
            ClearMessages();
            IsBusy = true;
            try
            {
                var result = await authService.ForgotPasswordAsync(Model);
                if (result?.Succeeded == true)
                    SetSuccess("If the email exists, a reset link has been sent.");
                else
                    SetError(result?.Message);
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private Task GoBackAsync() => Shell.Current.GoToAsync("..");
    }
}
