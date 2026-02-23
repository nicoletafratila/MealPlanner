namespace MealPlanner.UI.Web.Pages
{
    public interface IModalController
    {
        Task CloseAsync(object? data = null);
        Task CancelAsync();
    }
}
