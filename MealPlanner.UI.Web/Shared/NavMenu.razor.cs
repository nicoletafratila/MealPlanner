namespace MealPlanner.UI.Web.Shared
{
    public partial class NavMenu
    {
        private bool collapseNavMenu = true;

        private void ToggleNavMenu()
        {
            collapseNavMenu = !collapseNavMenu;
        }
    }
}
