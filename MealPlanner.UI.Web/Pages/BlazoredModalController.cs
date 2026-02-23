using Blazored.Modal;
using Blazored.Modal.Services;

namespace MealPlanner.UI.Web.Pages
{
    public class BlazoredModalController(BlazoredModalInstance instance) : IModalController
    {
        public Task CloseAsync(object? data = null)
            => instance.CloseAsync(ModalResult.Ok(data));

        public Task CancelAsync()
            => instance.CancelAsync();
    }
}
