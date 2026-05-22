using Common.UI;
using Identity.Shared.Models;
using MealPlanner.UI.Web.Services.Identities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages
{
    [AllowAnonymous]
    public partial class ContactUs
    {
        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        public ContactUsModel Model { get; } = new();

        private bool _isSending;

        [Inject]
        public IContactUsService ContactUsService { get; set; } = default!;

        private async Task OnSubmitAsync()
        {
            _isSending = true;
            try
            {
                var result = await ContactUsService.SendAsync(Model);

                if (result is null)
                {
                    await MessageComponent!.ShowErrorAsync(Resources.ContactUs.SubmitFailed);
                    return;
                }

                if (result.Succeeded)
                {
                    await MessageComponent!.ShowInfoAsync(result.Message ?? Resources.ContactUs.SubmitFailed);
                    Model.Name = string.Empty;
                    Model.EmailAddress = string.Empty;
                    Model.Subject = string.Empty;
                    Model.Message = string.Empty;
                }
                else
                {
                    await MessageComponent!.ShowErrorAsync(result.Message ?? Resources.ContactUs.SubmitFailed);
                }
            }
            finally
            {
                _isSending = false;
            }
        }
    }
}
