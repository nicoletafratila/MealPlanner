using BlazorBootstrap;
using Identity.Shared.Models;
using MealPlanner.UI.Web.Services;
using MealPlanner.UI.Web.Services.Identities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace MealPlanner.UI.Web.Pages.Identities
{
    [Authorize]
    public partial class UserProfile
    {
        private ConfirmDialog _dialog = default!;
        private List<BreadcrumbItem> _navItems = default!;
        private readonly long _maxFileSize = 1024L * 1024L * 1024L * 3L;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [Parameter]
        public string? Name { get; set; }
        public ApplicationUserEditModel? ApplicationUser { get; set; }

        [Inject]
        public IApplicationUserService? UserService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _navItems = new List<BreadcrumbItem>
            {
                new BreadcrumbItem{ Text = "Home", Href ="recipebooks/recipesoverview" }
            };

            if (string.IsNullOrWhiteSpace(Name))
            {
                ApplicationUser = new ApplicationUserEditModel();
            }
            else
            {
                ApplicationUser = await UserService!.GetEditAsync(Name);
            }
        }

        private async Task SaveAsync()
        {
            var response = await UserService!.UpdateAsync(ApplicationUser!);
            if (response != null && !response.Succeeded)
            {
                MessageComponent?.ShowError(response.Message!);
            }
            else
            {
                MessageComponent?.ShowInfo("Data has been saved successfully");
                NavigateToOverview();
            }
        }

        private void NavigateToOverview()
        {
            NavigationManager?.NavigateTo("recipebooks/recipesoverview");
        }

        private async Task OnInputFileChangeAsync(InputFileChangeEventArgs e)
        {
            try
            {
                if (e.File != null)
                {
                    Stream stream = e.File.OpenReadStream(maxAllowedSize: 1024 * 300);
                    MemoryStream ms = new();
                    await stream.CopyToAsync(ms);
                    stream.Close();
                    ApplicationUser!.ProfilePicture = ms.ToArray();
                }
                StateHasChanged();
            }
            catch (Exception)
            {
                MessageComponent?.ShowError($"File size exceeds the limit. Maximum allowed size is <strong>{_maxFileSize / (1024 * 1024)} MB</strong>.");
                return;
            }
        }
    }
}
