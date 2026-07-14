using Identity.Services.Http;
using Identity.Shared.Models;
using MealPlanner.UI.Mobile.Services;

namespace MealPlanner.UI.Mobile
{
    public partial class App : Application
    {
        private readonly MobileAuthStateService _authState;
        private readonly IServiceProvider _services;
        private readonly SecureStorageTokenProvider _tokenProvider;
        private static string? _pendingDeepLink;

        public App(MobileAuthStateService authState, IServiceProvider services,
            SecureStorageTokenProvider tokenProvider)
        {
            InitializeComponent();
            _authState = authState;
            _services = services;
            _tokenProvider = tokenProvider;
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
                    var (username, password) = await _tokenProvider.GetCredentialsAsync();
                    if (username != null && password != null)
                    {
                        var authService = _services.GetRequiredService<AuthenticationService>();
                        var result = await authService.LoginAsync(new LoginModel { Username = username, Password = password });
                        if (result?.Succeeded != true)
                            await Shell.Current.GoToAsync("//Login");
                    }
                    else
                    {
                        await Shell.Current.GoToAsync("//Login");
                    }
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
