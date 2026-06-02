using MealPlanner.UI.Mobile.Services;

namespace MealPlanner.UI.Mobile
{
    public partial class App : Application
    {
        private readonly MobileAuthStateService _authState;

        public App(MobileAuthStateService authState, IServiceProvider services)
        {
            InitializeComponent();
            _authState = authState;
            MainPage = services.GetRequiredService<AppShell>();
        }

        protected override async void OnStart()
        {
            base.OnStart();
            var isAuthenticated = await _authState.IsAuthenticatedAsync();
            if (!isAuthenticated)
                await Shell.Current.GoToAsync("//Login");
        }
    }
}
