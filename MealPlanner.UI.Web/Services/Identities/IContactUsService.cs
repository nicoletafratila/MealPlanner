using Common.Models;
using Identity.Shared.Models;

namespace MealPlanner.UI.Web.Services.Identities
{
    public interface IContactUsService
    {
        Task<CommandResponse?> SendAsync(ContactUsModel model, CancellationToken cancellationToken = default);
    }
}
