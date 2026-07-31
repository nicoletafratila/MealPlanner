using Identity.Services.Http;
using MealPlanner.UI.Mobile.Services;
using MealPlanner.UI.Mobile.ViewModels;

namespace MealPlanner.UI.Mobile
{
    public partial class App : Application
    {
        private readonly AuthenticationStateService _authState;
        private readonly IServiceProvider _services;
        private static string? _pendingDeepLink;

        public App(AuthenticationStateService authState, IServiceProvider services)
        {
            InitializeComponent();
            _authState = authState;
            _services = services;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(_services.GetRequiredService<AppShell>());
        }

        protected override async void OnStart()
        {
            base.OnStart();
            try
            {
                var isAuthenticated = await _authState.IsAuthenticatedAsync();
                if (!isAuthenticated)
                {
                    var authService = _services.GetRequiredService<AuthenticationService>();
                    isAuthenticated = await authService.RefreshAsync();
                }

                if (isAuthenticated)
                {
                    var appShellViewModel = _services.GetRequiredService<AppShellViewModel>();
                    await appShellViewModel.LoadCurrentCommand.ExecuteAsync(null);
                    await Shell.Current.GoToAsync("//RecipesOverview");
                }
                else
                {
                    await Shell.Current.GoToAsync("//Login");
                }

                await ProcessPendingDeepLinkAsync();
            }
            catch
            {
                await Shell.Current.GoToAsync("//Login");
            }
        }

        // Called from MainActivity when the app is already running (OnNewIntent)
        public static void HandleDeepLink(string url)
        {
            if (Shell.Current is not null)
            {
                MainThread.BeginInvokeOnMainThread(async () => await NavigateToDeepLinkAsync(url));
            }
            else
            {
                _pendingDeepLink = url;
            }
        }

        private static async Task ProcessPendingDeepLinkAsync()
        {
            if (_pendingDeepLink is null) return;
            var url = _pendingDeepLink;
            _pendingDeepLink = null;
            await NavigateToDeepLinkAsync(url);
        }

        private static async Task NavigateToDeepLinkAsync(string url)
        {
            var uri = new Uri(url);
            if (uri.Host == "reset-password")
                await Shell.Current.GoToAsync($"ResetPassword{uri.Query}");
        }
    }
}
