using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;
using Identity.Shared.Resources;
using MealPlanner.Shared.Resources;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class ForgotPasswordViewModel(AuthenticationService authService) : BaseViewModel
    {
        public ForgotPasswordModel Model { get; } = new();

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task SendAsync()
        {
            if (IsBusy) return;
            ClearMessages();

            if (string.IsNullOrWhiteSpace(Model.EmailAddress))
            {
                SetError(IdentitySharedMessages.EmailAddressRequired);
                return;
            }

            IsBusy = true;
            try
            {
                var result = await authService.ForgotPasswordAsync(Model);
                if (result?.Succeeded == true)
                    SetSuccess(MealPlannerSharedMessages.ForgotPasswordEmailSent);
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
        private Task GoBackAsync() => Shell.Current.GoToAsync("..");
    }
}
