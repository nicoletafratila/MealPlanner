using CommunityToolkit.Mvvm.Input;
using Identity.Services.Http;
using Identity.Shared.Models;
using Identity.Shared.Resources;
using MealPlanner.UI.Mobile.Pages.Identity.Resources;

namespace MealPlanner.UI.Mobile.ViewModels.Identity
{
    public partial class ContactUsViewModel(IContactUsService contactUsService) : BaseViewModel
    {
        public ContactUsModel Model { get; } = new();

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task SendAsync()
        {
            if (IsBusy) return;
            ClearMessages();

            if (string.IsNullOrWhiteSpace(Model.Name))
            {
                SetError(ContactUsPage.NameRequired);
                return;
            }

            if (string.IsNullOrWhiteSpace(Model.EmailAddress))
            {
                SetError(IdentitySharedMessages.EmailAddressRequired);
                return;
            }

            if (string.IsNullOrWhiteSpace(Model.Subject))
            {
                SetError(ContactUsPage.SubjectRequired);
                return;
            }

            if (string.IsNullOrWhiteSpace(Model.Message))
            {
                SetError(ContactUsPage.MessageRequired);
                return;
            }

            IsBusy = true;
            try
            {
                var result = await contactUsService.SendAsync(Model);
                if (result?.Succeeded == true)
                {
                    SetSuccess(result.Message ?? ContactUsPage.SubmitFailed);
                    Model.Name = string.Empty;
                    Model.EmailAddress = string.Empty;
                    Model.Subject = string.Empty;
                    Model.Message = string.Empty;
                }
                else
                {
                    SetError(result?.Message ?? ContactUsPage.SubmitFailed);
                }
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }
            finally { IsBusy = false; }
        }
    }
}
