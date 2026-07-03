using MealPlanner.UI.Mobile.Services;

namespace MealPlanner.UI.Mobile
{
    public partial class App : Application
    {
        private readonly MobileAuthStateService _authState;
        private readonly IServiceProvider _services;
        private static string? _pendingDeepLink;

        public App(MobileAuthStateService authState, IServiceProvider services)
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
            var isAuthenticated = await _authState.IsAuthenticatedAsync();
            if (!isAuthenticated)
                await Shell.Current.GoToAsync("//Login");
            await ProcessPendingDeepLinkAsync();
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
