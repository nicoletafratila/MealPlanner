using MealPlanner.UI.Mobile.ViewModels.RecipeBook;

namespace MealPlanner.UI.Mobile.Pages.RecipeBook
{
    public partial class RecipeDetailPage : ContentPage
    {
        private readonly RecipeDetailViewModel _viewModel;

        public RecipeDetailPage(RecipeDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            _viewModel.LoadCommand.Execute(null);
        }

        private void OnSourcePointerEntered(object sender, PointerEventArgs e)
        {
#if WINDOWS
            SetWindowsCursor(sender, Microsoft.UI.Input.InputSystemCursorShape.Hand);
#endif
        }

        private void OnSourcePointerExited(object sender, PointerEventArgs e)
        {
#if WINDOWS
            SetWindowsCursor(sender, Microsoft.UI.Input.InputSystemCursorShape.Arrow);
#endif
        }

#if WINDOWS
        private static void SetWindowsCursor(object sender, Microsoft.UI.Input.InputSystemCursorShape shape)
        {
            if (sender is VisualElement ve && ve.Handler?.PlatformView is Microsoft.UI.Xaml.UIElement uiElement)
            {
                var cursor = Microsoft.UI.Input.InputSystemCursor.Create(shape);
                typeof(Microsoft.UI.Xaml.UIElement)
                    .GetProperty("ProtectedCursor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(uiElement, cursor);
            }
        }
#endif
    }
}
