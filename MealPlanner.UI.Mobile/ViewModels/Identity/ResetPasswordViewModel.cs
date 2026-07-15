using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;
using MealPlanner.Shared.Resources;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class ResetPasswordViewModel(AuthenticationService authService) : BaseViewModel, IQueryAttributable
    {
        public ResetPasswordModel Model { get; } = new();

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("userId", out var userId))
                Model.UserId = Uri.UnescapeDataString(userId.ToString()!);
            if (query.TryGetValue("token", out var token))
                Model.Token = Uri.UnescapeDataString(token.ToString()!);
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task ResetAsync()
        {
            if (IsBusy) return;
            ClearMessages();
            if (Model.NewPassword != Model.ConfirmPassword)
            {
                SetError(MealPlannerSharedMessages.PasswordsDoNotMatch);
                return;
            }
            IsBusy = true;
            try
            {
                var result = await authService.ResetPasswordAsync(Model);
                if (result?.Succeeded == true)
                {
                    SetSuccess(MealPlannerSharedMessages.PasswordResetSuccess);
                    await Task.Delay(1500);
                    await Shell.Current.GoToAsync("..");
                }
                else SetError(result?.Message);
            }
            finally { IsBusy = false; }
        }
    }
}
