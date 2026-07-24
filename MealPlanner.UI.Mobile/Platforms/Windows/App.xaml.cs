using Microsoft.Windows.AppLifecycle;
using Windows.ApplicationModel.Activation;

namespace MealPlanner.UI.Mobile.WinUI
{
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
            InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            if (activatedArgs?.Kind == ExtendedActivationKind.Protocol &&
                activatedArgs.Data is IProtocolActivatedEventArgs protocolArgs &&
                protocolArgs.Uri.Scheme == "mealplanner")
            {
                Mobile.App.HandleDeepLink(protocolArgs.Uri.ToString());
            }
        }
    }
}
