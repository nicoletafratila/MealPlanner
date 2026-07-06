using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace MealPlanner.UI.Mobile.Platforms.Android
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
                               ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
                               ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    [IntentFilter(
        [Intent.ActionView],
        Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
        DataScheme = "mealplanner",
        DataHost = "reset-password")]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HandleIntent(Intent);
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            Intent = intent;
            HandleIntent(intent);
        }

        private static void HandleIntent(Intent? intent)
        {
            var url = intent?.Data?.ToString();
            if (intent?.Action == Intent.ActionView && url?.StartsWith("mealplanner://") == true)
                App.HandleDeepLink(url);
        }
    }
}
