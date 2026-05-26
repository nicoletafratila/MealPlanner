using BlazorBootstrap;
using Common.UI;
using Identity.Shared.Models;
using Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;

namespace MealPlanner.UI.Web.Pages.Identities
{
    [Authorize]
    public partial class UserProfile
    {
        private List<BreadcrumbItem> _navItems = default!;
        private readonly long _maxFileSize = 1024L * 1024L * 1024L * 3L;

        [CascadingParameter(Name = "MessageComponent")]
        private IMessageComponent? MessageComponent { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

        [Parameter]
        public string? Name { get; set; }
        public ApplicationUserEditModel? ApplicationUser { get; set; }

        [Inject]
        public IApplicationUserService? ApplicationUserService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _navItems = new List<BreadcrumbItem>
            {
                new BreadcrumbItem{ Text = Resources.UserProfile.BreadcrumbHome, Href ="recipebooks/recipesoverview" }
            };

            if (string.IsNullOrWhiteSpace(Name))
            {
                ApplicationUser = new ApplicationUserEditModel();
            }
            else
            {
                ApplicationUser = await ApplicationUserService!.GetEditAsync(Name);
            }
        }

        private async Task SaveAsync()
        {
            var response = await ApplicationUserService!.UpdateAsync(ApplicationUser!);
            if (response != null && !response.Succeeded)
            {
                await MessageComponent!.ShowErrorAsync(response.Message!);
            }
            else
            {
                await MessageComponent!.ShowInfoAsync(Resources.UserProfile.SaveSucceeded);
                await NavigateToOverview();
            }
        }

        private async Task NavigateToOverview()
        {
            var authState = await AuthenticationStateTask;
            var destination = authState.User.IsInRole("admin")
                ? "identities/usersoverview"
                : "recipebooks/recipesoverview";

            NavigationManager?.NavigateTo(destination);
        }

        private async Task UnlockUserAsync()
        {
            var response = await ApplicationUserService!.UnlockAsync(ApplicationUser!.UserId!);
            if (response != null && !response.Succeeded)
            {
                await MessageComponent!.ShowErrorAsync(response.Message!);
            }
            else
            {
                ApplicationUser!.IsLockedOut = false;
                await MessageComponent!.ShowInfoAsync(Resources.UserProfile.UnlockSucceeded);
                StateHasChanged();
            }
        }

        private void NavigateToChangePassword()
        {
            NavigationManager?.NavigateTo($"identities/change-password?userId={ApplicationUser?.UserId}&name={ApplicationUser?.Username}");
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
                await MessageComponent!.ShowErrorAsync(string.Format(Resources.UserProfile.FileSizeExceeded, _maxFileSize / (1024 * 1024)));
                return;
            }
        }
    }
}
