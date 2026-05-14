using BlazorBootstrap;
using Common.UI;
using Identity.Shared.Models;
using MealPlanner.UI.Web.Services.Identities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
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
                NavigateToOverview();
            }
        }

        private void NavigateToOverview()
        {
            NavigationManager?.NavigateTo("identities/usersoverview");
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
