namespace MealPlanner.UI.Web.Services
{
    public interface IMessageComponent
    {
        void ShowError(string message);
        void ShowInfo(string message);
    }
}
