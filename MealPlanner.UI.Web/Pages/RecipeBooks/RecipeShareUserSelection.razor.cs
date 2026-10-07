using Blazored.Modal;
using Identity.Services.Http;
using Identity.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Web.Pages.RecipeBooks
{
    [Authorize]
    public partial class RecipeShareUserSelection
    {
        [CascadingParameter]
        public BlazoredModalInstance? ModalInstance { get; set; }

        public IModalController? ModalController { get; set; }

        public IList<ApplicationUserListModel>? Users { get; private set; }

        public string? TargetUserId { get; set; }

        [Inject]
        public IApplicationUserService ApplicationUserService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            Users = await ApplicationUserService.ListAsync() ?? [];
        }

        protected override void OnParametersSet()
        {
            if (ModalController is null && ModalInstance is not null)
            {
                ModalController = new BlazoredModalController(ModalInstance);
            }
        }

        private static string DisplayName(ApplicationUserListModel user)
        {
            var fullName = string.Join(" ", new[] { user.FirstName, user.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
            return string.IsNullOrWhiteSpace(fullName) ? user.Username : $"{fullName} ({user.Username})";
        }

        private async Task SaveAsync()
        {
            await ModalController!.CloseAsync(TargetUserId);
        }

        private async Task CancelAsync()
        {
            await ModalController!.CancelAsync();
        }
    }
}
