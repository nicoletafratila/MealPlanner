using MealPlanner.UI.Mobile.Pages.Identity;
using MealPlanner.UI.Mobile.Services;

namespace MealPlanner.UI.Mobile
{
    public partial class App : Application
    {
        private readonly MobileAuthStateService _authState;
        private readonly IServiceProvider _services;

        public App(MobileAuthStateService authState, IServiceProvider services)
        {
            InitializeComponent();
            _authState = authState;
            _services = services;
            MainPage = new NavigationPage(services.GetRequiredService<LoginPage>());
        }

        protected override async void OnStart()
        {
            base.OnStart();
            var isAuthenticated = await _authState.IsAuthenticatedAsync();
            if (isAuthenticated)
                Windows[0].Page = _services.GetRequiredService<AppShell>();
        }
    }
}
