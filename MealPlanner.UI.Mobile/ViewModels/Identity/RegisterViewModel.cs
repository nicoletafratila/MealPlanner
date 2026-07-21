using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;
using Identity.Shared.Resources;
using MealPlanner.Shared.Resources;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class RegisterViewModel(AuthenticationService authService) : BaseViewModel
    {
        public RegistrationModel Model { get; } = new();

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task RegisterAsync()
        {
            if (IsBusy) return;
            ClearMessages();

            if (string.IsNullOrWhiteSpace(Model.Username))
            {
                SetError(IdentitySharedMessages.UsernameRequired);
                return;
            }

            if (string.IsNullOrWhiteSpace(Model.EmailAddress))
            {
                SetError(IdentitySharedMessages.EmailAddressRequired);
                return;
            }

            if (string.IsNullOrWhiteSpace(Model.ConfirmPassword))
            {
                SetError(IdentitySharedMessages.ConfirmPasswordRequired);
                return;
            }

            if (Model.Password != Model.ConfirmPassword)
            {
                SetError(MealPlannerSharedMessages.PasswordsDoNotMatch);
                return;
            }
            IsBusy = true;
            try
            {
                var result = await authService.RegisterAsync(Model);
                if (result?.Succeeded == true)
                {
                    SetSuccess(MealPlannerSharedMessages.RegistrationSuccess);
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
